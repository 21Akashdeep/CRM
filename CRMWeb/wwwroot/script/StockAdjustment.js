class StockAdjustment {
    static item = [];
    static itemWithSerialNo = [];
    static reasonCode = [];
    static init() {

        StockAdjustment.getViewOption();

        $('#btnNewEntry').on('click', () => {
            StockAdjustment.newEntry();
        });

        $('#btnSearch').on('click', () => {
            StockAdjustment.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableStockAdjustment', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            StockAdjustment.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableStockAdjustment', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            StockAdjustment.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: 'StockAdjustment' });
                }
            });
        });
        $('#StockAdjustmentScan-ItemSerialNo').on('keydown', (e) => {
            if (e.key == 'Enter') {
                if (!Field.isMandatory({ class: '.scan-required' })) {
                    return;
                }
                let serialNo = $('#StockAdjustmentScan-ItemSerialNo').val();
                let countStockAdjustmentItem = $('#tableStockAdjustmentItem').bootstrapTable('getData').filter(x => x.SerialNo == serialNo).length;
                let countScanItem = $('#tableStockAdjustmentScanItem').bootstrapTable('getData').filter(x => x.SerialNo == serialNo).length;
                if (countStockAdjustmentItem > 0 || countScanItem > 0) {
                    Message.error({ statusText: `Serial No. ${serialNo} already added in Scan List or Item List.` });
                    return;
                }
                StockAdjustment.addStockAdjustmentItem({
                    itemId: $('#StockAdjustmentScan-ItemId').val(),
                    itemDesc: $('#StockAdjustmentScan-ItemId option:selected').text(),
                    expiryOn: $('#StockAdjustmentScan-ItemExpiryOn').val(),
                    reasonCode: $('#StockAdjustmentScan-ReasonCode').val(),
                    reasonCodeDesc: $('#StockAdjustmentScan-ReasonCode option:selected').text(),
                    serialNo: serialNo,
                    qty: 1,
                    isScanned: true,
                    callback: (obj) => {
                        Table.add({ id: '#tableStockAdjustmentScanItem', data: obj, action: 'prepend' });
                        $('#StockAdjustmentScan-ItemExpiryOn,#StockAdjustmentScan-ItemSerialNo').val('');
                    }
                });
            }
        });

        $('#btnScanItemAdd').on('click', () => {
            let scanItems = $('#tableStockAdjustmentScanItem').bootstrapTable('getData');
            if (scanItems.length === 0) {
                Message.error({ statusText: 'No scanned items found' });
                return;
            }
            Table.add({ id: '#tableStockAdjustmentItem', data: scanItems, action: 'append', selectPick: true });
            StockAdjustment.sumOfTotalStockAdjustmentItem();
            Modal.close({ id: '#modalStockAdjustmentItemScan' });
        });
        $('#StockAdjustment-StoreId').on('change', () => {

            let obj = {
                ListStoreId: [$('#StockAdjustment-StoreId').val()],
            };
            Data.post({
                url: 'StockAdjustment/GetStockItem',
                data: obj,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        Gdn.item = response.data;
                    }
                    else {
                        Message.show(response);
                    }
                }
            });
        });
        $('#StockAdjustment-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let StockAdjustmentItem = $('#tableStockAdjustmentItem').bootstrapTable('getData').filter(x => x.ItemId > 0)
                .map(x => {
                    if (x.ExpiryOn) {
                        x.ExpiryOn = moment(x.ExpiryOn).format('YYYY-MM-DD');
                    }
                    return x;
                });
            let obj = Data.serializeToObject({ formId: '#formStockAdjustment' });
            obj.VoucherItem = StockAdjustmentItem;

            if (!obj.Id) {
                StockAdjustment.add(obj);
            }
            else {
                StockAdjustment.update(obj);
            }
        });
    }
    static getViewOption() {
        Data.get({
            url: 'StockAdjustment/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListStockAdjustmentNo', data: response.data.StockAdjustmentNo, value: 'Id', text: 'No' })
            }
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'StockAdjustment/GetAddOption', onSuccess: onSuccess });
    }
    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListNo: $('#ListStockAdjustmentNo').val()
        };
        Data.post({
            url: `StockAdjustment/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static newEntry() {
        StockAdjustment.getAddOption({
            onSuccess: (response) => {
                StockAdjustment.item = response.data.Item;
                let store = response.data.Store;
                StockAdjustment.reasonCode = response.data.ReasonCode;
                Dropdown.bind({ id: '#StockAdjustment-StoreId', data: store, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockAdjustment', title: 'StockAdjustment / Add', action: 'Add' });
                $('#StockAdjustment-RefDate,#StockAdjustment-EwayDate').val('');
            }
        });
    }
    static scanner() {
        Modal.open({ id: '#modalStockAdjustmentItemScan', title: 'StockAdjustment / Scan Item' });
        Dropdown.bind({ id: '#StockAdjustmentScan-ItemId', data: StockAdjustment.item, value: 'Id', text: 'Description' });
        Dropdown.bind({ id: '#StockAdjustmentScan-ReasonCode', data: StockAdjustment.reasonCode, value: 'Value', text: 'Description' });
        $('#StockAdjustment-ExpiryOn,#StockAdjustmentScan-ItemSerialNo').val('');

    }
    static addStockAdjustmentItem({ itemId = 0, itemDesc = null, serialNo = "", expiryOn = null, qty = 0, reasonCode = "", reasonCodeDesc = "", unitDesc = null, isScanned = false, callback } = {}) {

        if (Field.isNullOrEmpty($('#StockAdjustment-StoreId').val())) {
            Message.error({ statusText: 'Store is not select.' });
            return;
        }

        let obj = {
            Id: 0,
            ItemId: itemId,
            VoucherId: 0,
            StoreId: 0,
            ItemDesc: itemDesc,
            SerialNo: serialNo,
            BatchNo: "",
            ExpiryOn: expiryOn,
            Qty: qty,
            Rate: 0,
            UnitDesc:unitDesc,
            UnitDesc: null,
            Amount: 0,
            DiscountAmount: 0,
            TotalAmount: 0,
            ListTax: "",
            TaxRate: 0,
            TaxAmount: 0,
            GrossAmount: 0,
            ImageUrl: null,
            ReasonCode: reasonCode,
            ReasonCodeDesc: reasonCodeDesc,
            Remarks: null,
            IsScanned: isScanned
        };
        if (!callback) {
            Table.add({ id: '#tableStockAdjustmentItem', data: obj, action: 'append', selectPick: true });
            StockAdjustment.sumOfTotalStockAdjustmentItem();
        }
        else {
            callback(obj);
        }
    }
    static addStockAdjustmentScanItem({ ItemId, SerialNo, ExpiryOn,ReasonCode, Qty }) {
        Table.add({ id: '#tableStockAdjustmentScanItem', data: { ItemId, SerialNo, ExpiryOn,ReasonCode, Qty }, action: 'append' });
    }
    static sumOfTotalStockAdjustmentItem() {
        let StockAdjustmentItem = $('#tableStockAdjustmentItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
        let Qty = StockAdjustmentItem.length == 0 ? 0 : StockAdjustmentItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);
        let Amount = StockAdjustmentItem.length == 0 ? 0 : StockAdjustmentItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="6">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>                    
                </tr>
            </tfoot>
        `;
        $('#tableStockAdjustmentItem tfoot').remove();
        $('#tableStockAdjustmentItem').append(tfoot);
    }
    static add(obj) {
        Data.post({
            url: 'StockAdjustment/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalStockAdjustment' });
                    Table.add({ id: '#tableStockAdjustment', data: response.obj, action: 'prepend' });
                    //Dropdown.bind({ id: '#ListId', data: response.data.ViewOption.StockAdjustment, value: 'Id', text: 'StockAdjustmentNo', subText: 'Date' });
                }
            }
        });
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `StockAdjustment/Edit?Id=${id}`,
            onSuccess: (response) => {
                let option = response.data;
                StockAdjustment.item = option.Item;
                let store = response.data.Store;
                let state = response.data.State;
                let obj = response.obj;
                let StoreId = obj.VoucherItem[0].StoreId;
                obj.StoreId = StoreId;
               // obj.reasonCode = ReasonCode;

                obj.Id = action == 'Edit' ? obj.Id : null;
                let title = action === 'Edit' ? `StockAdjustment / Edit (Code: ${obj.No})` : `StockAdjustment / Add`;
                Dropdown.bind({ id: '#StockAdjustment-StoreId', data: store, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockAdjustment', title: title, action: action, obj: obj });
                Table.add({ id: '#tableStockAdjustmentItem', data: obj.VoucherItem, selectPick: true });
                StockAdjustment.sumOfTotalStockAdjustmentItem();


            }
        });
    }
    static update(obj) {
        Data.update({
            url: 'StockAdjustment/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalStockAdjustment' });
                    Table.updateById({ id: '#tableStockAdjustment', objId: response.obj.Id, obj: response.obj });
                }
            }
        });
    }
    static delete({ id }) {
        Message.confirm(
            {
                msg: "Do you want to delete???",
                confirmButtonText: "Delete",
                denyButtonText: "Don't Delete",
                data: id,
                onConfirm: (id) => {
                    Data.delete(
                        {
                            url: `StockAdjustment/Delete?Id=${id}`,
                            onSuccess: StockAdjustment.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableTask", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }
    static deleteItem({ id, index }) {
        Table.remove({ id: '#tableStockAdjustmentItem', value: [index] });
    }
    static deleteScanItem({ id, index }) {
        Table.remove({ id: '#tableStockAdjustmentScanItem', value: [index] });
    }
    static print({ obj }) {
        Data.post({
            url: "StockAdjustment/Print",
            data: obj,
            onSuccess: (response) => {
                if (response.status !== Message.Type.success) {
                    Message.error(response);
                    return;
                }

                let reportTitle = 'Stock Adjustment';
                if (obj.FromDate && obj.TillDate) {
                    reportTitle += ` ${moment(obj.FromDate).format('DD-MMM-YYYY')} To ${moment(obj.TillDate).format('DD-MMM-YYYY')}`;
                }

                let printContent = [];
                let grnList = Array.isArray(response.data) ? response.data : [];

                grnList.forEach((obj, index) => {
                    let table = [];
                    table.push('<table class="table table-bordered">');

                    // ===== Header =====
                    table.push(`
            <thead>
                <tr>
                    <th colspan="8">
                        <div class="text-center brand-name">${App.Company.Description}</div>
                        <div class="text-center">${reportTitle}</div>
                    </th>
                </tr>
                <tr>
                    <th>SL No.</th>
                    <th>StockAdjustment No.</th>
                    <th>Date</th>
                    <th> From Store</th>
                    <th>Party</th>
                    <th>Ref No</th>
                    <th>Remarks</th>
                    <th>Created By</th>
                </tr>
            </thead>
            `);

                    table.push('<tbody>');

                    // ===== GRN Master Row =====
                    table.push(`
            <tr>
                <td class="text-center">${index + 1}</td>
                <td>${obj.No}</td>
                <td>${moment(obj.Date).format('DD-MMM-YYYY')}</td>
                <td>${obj.StoreDesc || '-'}</td>
                <td>${obj.PartyDesc || '-'}</td>
                <td>${obj.RefNo || '-'}</td>                
                <td>${obj.Remarks || '-'}</td>
                <td>${obj.CreatedByName}</td>
            </tr>
            `);

                    // ===== GRN Items =====
                    let items = obj.VoucherItem || [];

                    table.push(`
            <tr>
                <td colspan="10" style="border-bottom:none;">
                    <table>
                        <thead>
                            <tr>
                                <th colspan="7">StockAdjustment Items</th>
                            </tr>
                            <tr>
                                <th>SL No.</th>
                                <th>Item</th>                                  
                                <th>Expiry</th> 
                                <th>Serial No</th>
                                <th class="text-right">Qty</th>
                                
                            </tr>
                        </thead>
                        <tbody>
                            ${items.map((row, idx) => `
                                <tr>
                                    <td class="text-center">${idx + 1}</td>
                                    <td>${row.ItemDesc}</td>                                     
                                    <td>${row.ExpiryOn ? moment(row.ExpiryOn).format('DD-MMM-YYYY') : '-'}</td> 
                                     <td>${row.SerialNo}</td>
                                    <td class="text-right">${row.Qty}</td>
                                </tr>
                            `).join('')}
                            <tr>
                                <th colspan="4" class="text-right">Total</th>
                                <th class="text-right">
                                    ${items.reduce((s, x) => s + Number(x.Qty || 0), 0)}
                                </th>                                
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
            `);

                    table.push('</tbody>');
                    table.push('</table>');

                    printContent.push(`
            <div class="page-container">
                <div class="page">
                    ${table.join('')}
                </div>
            </div>
            `);
                });

                // ===== Styles =====
                let style = [];
                style.push(`
            table {
                border-collapse: collapse;
                width: 100%;
            }
            table td, table th {
                border: 1px solid black;
                padding: 5px;
            }
            .text-right {
                text-align: right;
            }
            .text-center {
                text-align: center;
            }
        `);

                Print.page({
                    title: reportTitle,
                    style: style,
                    content: printContent,
                    border: false,
                    orientation: 'A4 landscape',
                    isPrint: false
                });
            }
        });
    }
}
window.tableStockAdjustmentRefNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.RefNo ?? ''}</div>
        <div>${obj.RefDate ? moment(obj.RefDate).format('DD-MMM-YYYY') : ''}</div>
    `;
};
window.tableStockAdjustmentSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableStockAdjustmentDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableStockAdjustmentStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableStockAdjustmentCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
}
window.tableStockAdjustmentUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${DateTime.dateTime(obj.UpdatedAt)}</div>
    `;
}
window.tableStockAdjustmentAction = (value, obj, index) => {
    let actionBtn = [];

    if (obj.IsEdit) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                        </a>
            </li>
        `);
    }
    if (obj.IsDelete) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-danger-100 btn-delete" title="Delete">
                    <span class="fa fa-trash text-danger-100"></span>&nbsp;&nbsp;Delete
                </a>
            </li>
        `);
    }
    if (obj.IsPrint) {
        actionBtn.push(
            `
     <li>
        <a href="#" class="dropdown-item text-primary-100 btn-print" title="Print">
            <span class="fa fa-print text-primary-100"></span>&nbsp;&nbsp;Print
        </a>
    </li>
            `
        );
    }

    return `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
            ${actionBtn.join('')}
            </ul>
        </div>
    `;
}
window.tableStockAdjustmentConAddress = (value, obj, index) => {

    const parts = [
        obj.ConAdd1,
        obj.ConAdd2,
        obj.ConPostOffice,
        obj.ConPincode,
        obj.ConStateName
    ]
        .filter(x => x && x.trim() !== "")
        .map(x => x.trim());

    if (parts.length === 0) return "";

    let lines = [];
    for (let i = 0; i < parts.length; i += 2) {
        lines.push(parts.slice(i, i + 2).join(", "));
    }

    return lines.join("<br>");
};
window.tableStockAdjustmentActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        StockAdjustment.edit({ id: obj.Id, action: 'Edit' });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        StockAdjustment.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        StockAdjustment.print({ obj: { ListId: [obj.Id] } });
    },
    'click .btn-delete': (e, value, obj, index) => {
        StockAdjustment.delete({ id: obj.Id });
    }
}
//Table StockAdjustment Item
window.tableStockAdjustmentItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableStockAdjustmentItemDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockAdjustmentItem_${index}`, className: 'StockAdjustment-item', data: StockAdjustment.item, value: 'ItemId', text: 'ItemDesc', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
        <input type="text" id="StockAdjustmentItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 StockAdjustment-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
    `;
}
window.tableStockAdjustmentItemDescEvent = {
    'change .StockAdjustment-item': (e, value, obj, index) => {
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        //Set Unit & Rate
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson?.UnitDesc ?? null;
        obj.UnitId = itemJson?.UnitId ?? 0;
        obj.Rate = itemJson?.Rate ?? 0;
       
        //obj.ItemId = itemJson?.ItemId ?? 0;
        // Set Serial No. & BalQty
        let item = StockAdjustment.item.find(x => x.ItemId == obj.ItemId);
        item.SerialNo ? obj.SerialNo = item.SerialNo : obj.SerialNo = "N/A";
        obj.BalQty = item ? item.Qty : 0;
        //Sum of Amount
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

        Table.updateByIndex({ id: '#tableStockAdjustmentItem', index: index, obj: obj, value: obj.ItemId, event: e });
        StockAdjustment.sumOfTotalStockAdjustmentItem();
    },
    'input .StockAdjustment-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableStockAdjustmentItem', index: index, obj: obj, value: obj.Remarks, event: e });
    }
}
window.tableStockAdjustmentItemRate = (value, obj, index) => {
    return `
        <input type="text" id="StockAdjustmentItemRate_${index}" class="form-control form-control-sm text-right StockAdjustment-item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableStockAdjustmentItemRateEvent = {
    'input .StockAdjustment-item-rate': (e, value, obj, index) => {
        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableStockAdjustmentItem', index: index, obj: obj, value: obj.Rate, event: e });
        StockAdjustment.sumOfTotalStockAdjustmentItem();
    }
}
window.tableStockAdjustmentItemQty = (value, obj, index) => {
    return `
        <input type="text" id="StockAdjustmentItemQty_${index}" class="form-control form-control-sm text-right StockAdjustment-item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableStockAdjustmentItemQtyEvent = {
    'input .StockAdjustment-item-qty': (e, value, obj, index) => {
        obj.Qty = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableStockAdjustmentItem', index: index, obj: obj, value: obj.Qty, event: e });
        StockAdjustment.sumOfTotalStockAdjustmentItem();
    }
}
window.tableStockAdjustmentReasonCodeDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockAdjustmentReasonCode_${index}`, className: 'StockAdjustment-item', data: StockAdjustment.reasonCode, value: 'Value', text: 'Description', initialValue: [obj.ReasonCode], json: true, parent: '.modal' })}
       
    `;
}
window.tableStockAdjustmentReasonCodeDescEvent = {
    'change .StockAdjustment-item': (e, value, obj, index) => {
        obj.ReasonCode = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockAdjustmentItem',
            index: index,
            obj: obj,
            value: obj.ReasonCode,
            event: e
        });
    }
};
window.tableStockAdjustmentStoreDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockAdjustmentStore_${index}`, className: 'StockAdjustment-item', data: StockAdjustment.store, value: 'Id', text: 'Description', initialValue: [obj.StoreId], json: true, parent: '.modal' })}
       
    `;
}
window.tableStockAdjustmentStoreDescEvent = {
    'change .StockAdjustment-item': (e, value, obj, index) => {
        obj.StoreId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        Table.updateByIndex({
            id: '#tableStockAdjustmentItem',
            index: index,
            obj: obj,
            value: obj.StoreId,
            event: e
        });
    }
};
window.tableStockAdjustmentItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableStockAdjustmentItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockAdjustment.deleteItem({ id: obj.Id, index: index });
    }
}
window.tableStockAdjustmentExpiryOn = (value, obj) => {
    return `${!obj.ExpiryOn ? '-' : moment(obj.ExpiryOn).format('DD-MM-YYYY')}`;
};

 //Table Scan Item
window.tableScanExpiry = (v, obj) => {
    return obj.ExpiryOn
        ? moment(obj.ExpiryOn).format('DD-MMM-YYYY')
        : '';
};
window.tableScanQty = (v, obj) => {
    return obj.Qty;
};
window.tableScanSerial = (v, obj) => {
    return obj.SerialNo;
};
window.tableStockAdjustmentScanItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableStockAdjustmentScanItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockAdjustment.deleteScanItem({ id: obj.Id, index: index });
    }
}
window.tableStockItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableStockAdjustmentItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockAdjustment.deleteItem({ id: obj.Id, index: index });
    }
}
window.tableStockAdjustmentReasonCodeDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockAdjustmentReasonCode_${index}`, className: 'StockAdjustment-reasoncode', data: StockAdjustment.reasonCode, value: 'Value', text:'Description', initialValue: [obj.ReasonCode], json: true, parent: '.modal' })}       
    `;
}
window.tableStockAdjustmentReasonCodeDescEvent = {
    'change .StockAdjustment-reasoncode': (e, value, obj, index) => {
        const val = e.currentTarget.value || '';
        obj.ReasonCode = val;
        const item = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.ReasonCodeDesc = item?.ReasonCodeDesc ?? null;
        Table.updateByIndex({
            id: '#tableStockAdjustmentItem',
            index,
            obj,
            event: e
        });

        StockAdjustment.sumOfTotalStockAdjustmentItem();
    }
}
window.tabelItemName = (value, obj, index) => {
    if (!obj.ItemName || obj.ItemName.length === 0) {
        return '-';
    }

    return `
        <ol class="mb-0 ps-3">
            ${obj.ItemName.map(name => `<li>${name}</li>`).join('')}
        </ol>
    `;
}