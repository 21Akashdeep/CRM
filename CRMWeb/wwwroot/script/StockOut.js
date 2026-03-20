class StockOut {
    static item = [];
    static init() {

        StockOut.getViewOption();

        $('#btnNewEntry').on('click', () => {
            StockOut.newEntry();
        });

        $('#btnSearch').on('click', () => {
            StockOut.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableStockOut', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            StockOut.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableStockOut', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            StockOut.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: 'StockOut' });
                }
            });
        });
        $('#StockOutScan-ItemSerialNo').on('keydown', (e) => {
            if (e.key == 'Enter') {
                if (!Field.isMandatory({ class: '.scan-required' })) {
                    return;
                }
                let serialNo = $('#StockOutScan-ItemSerialNo').val();
                let countStockOutItem = $('#tableStockOutItem').bootstrapTable('getData').filter(x => x.SerialNo == serialNo).length;
                let countScanItem = $('#tableStockOutScanItem').bootstrapTable('getData').filter(x => x.SerialNo == serialNo).length;
                if (countStockOutItem > 0 || countScanItem > 0) {
                    Message.error({ statusText: `Serial No. ${serialNo} already added in Scan List or Item List.` });
                    return;
                }
                StockOut.addStockOutItem({  
                    itemId: $('#StockOutScan-ItemId').val(),
                    itemDesc: $('#StockOutScan-ItemId option:selected').text(),
                    expiryOn: $('#StockOutScan-ItemExpiryOn').val(),
                    serialNo: serialNo,
                    qty: 1,
                    isScanned: true,
                    callback: (obj) => {
                        Table.add({ id: '#tableStockOutScanItem', data: obj, action: 'prepend' });
                        $('#StockOutScan-ItemExpiryOn,#StockOutScan-ItemSerialNo').val('');

                    }
                });
            }
        });

        $('#btnScanItemAdd').on('click', () => {
            let scanItems = $('#tableStockOutScanItem').bootstrapTable('getData');
            if (scanItems.length === 0) {
                Message.error({ statusText: 'No scanned items found' });
                return;
            }
            Table.add({ id: '#tableStockOutItem', data: scanItems, action: 'append', selectPick: true });
            StockOut.sumOfTotalStockOutItem();

            Modal.close({ id: '#modalStockOutItemScan' });
        });

        $('#StockOut-StoreId').on('change', () => {
            StockOut.getItemDetails();
        });

        $('#StockOut-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let StockOutItem = $('#tableStockOutItem').bootstrapTable('getData').filter(x => x.ItemId > 0)
                .map(x => {
                    if (x.ExpiryOn) {
                        x.ExpiryOn = moment(x.ExpiryOn).format('YYYY-MM-DD');
                    }
                    return x;
                });
            let obj = Data.serializeToObject({ formId: '#formStockOut' });
            obj.VoucherItem = StockOutItem;

            if (!obj.Id) {
                StockOut.add(obj);
            }
            else {
                StockOut.update(obj);
            }
        });
    }
    static getViewOption() {
        Data.get({
            url: 'StockOut/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListStockOutNo', data: response.data.StockOutNo, value: 'Id', text: 'No' })
            }
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'StockOut/GetAddOption', onSuccess: onSuccess });
    }
    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListNo: $('#ListStockOutNo').val()
        };
        Data.post({
            url: `StockOut/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static newEntry() {
        StockOut.getAddOption({
            onSuccess: (response) => {
                StockOut.item = response.data.Item;
                let store = response.data.Store;

                Dropdown.bind({ id: '#StockOut-StoreId', data: store, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockOut', title: 'StockOut / Add', action: 'Add' });
                $('#StockOut-RefDate,#StockOut-EwayDate').val('');
            }
        });
    }
    static scanner() {
        let store = $('#StockOut-StoreId').val();

        if (!store || store.length == 0) {
            Message.error({ statusText: 'First Select Store' });
            return;
        }
        Modal.open({ id: '#modalStockOutItemScan', title: 'StockOut / Scan Item' });
        Dropdown.bind({ id: '#StockOutScan-ItemId', data: StockOut.item, value: 'Id', text: 'Description' });
        $('#StockOut-ExpiryOn,#StockOutScan-ItemSerialNo').val('');

    }
    static getItemDetails() {
        let obj = {
            StoreId: $('#StockOut-StoreId').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            Todate: DateTime.json($('#DateRange').val().split('|')[1])

        };
        Data.post({
            url: 'StockOut/getItem',
            data: obj,
            onSuccess: (response) => {
                if (!response.data || response.data.length === 0)
                    return;
                StockOut.item = response.data.data;

            }
        });
    }
    static addStockOutItem({ itemId = 0, itemDesc = null, serialNo = "", expiryOn = null, qty = 0, isScanned = false, callback } = {}) {

        let store = $('#StockOut-StoreId').val();

        if (!store || store.length == 0) {
            Message.error({ statusText: 'First Select Store' });
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
            UnitDesc: null,
            Amount: 0,
            DiscountAmount: 0,
            TotalAmount: 0,
            ListTax: "",
            TaxRate: 0,
            TaxAmount: 0,
            GrossAmount: 0,
            ImageUrl: null,
            ReasonCode: null,
            Remarks: null,
            IsScanned: isScanned
        };
        if (!callback) {
            Table.add({ id: '#tableStockOutItem', data: obj, action: 'append', selectPick: true });
            StockOut.sumOfTotalStockOutItem();
        }
        else {
            callback(obj);
        }
    }
    static addStockOutScanItem({ ItemId, SerialNo, ExpiryOn, Qty }) {
        Table.add({ id: '#tableStockOutScanItem', data: { ItemId, SerialNo, ExpiryOn, Qty }, action: 'append' });
    }
    static sumOfTotalStockOutItem() {
        let StockOutItem = $('#tableStockOutItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
        let Qty = StockOutItem.length == 0 ? 0 : StockOutItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);
        let Amount = StockOutItem.length == 0 ? 0 : StockOutItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="4">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>                    
                </tr>
            </tfoot>
        `;
        $('#tableStockOutItem tfoot').remove();
        $('#tableStockOutItem').append(tfoot);
    }
    static add(obj) {
        Data.post({
            url: 'StockOut/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalStockOut' });
                    Table.add({ id: '#tableStockOut', data: response.obj, action: 'prepend' });
                    //Dropdown.bind({ id: '#ListId', data: response.data.ViewOption.StockOut, value: 'Id', text: 'StockOutNo', subText: 'Date' });
                }
            }
        });
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `StockOut/Edit?Id=${id}`,
            onSuccess: (response) => {
                let option = response.data;
                StockOut.item = option.Item;
                let store = response.data.Store;
                let state = response.data.State;
                let obj = response.obj;
                let StoreId = obj.VoucherItem[0].StoreId;
                obj.StoreId = StoreId;
                obj.Id = action == 'Edit' ? obj.Id : null;
                let title = action === 'Edit' ? `StockOut / Edit (Code: ${obj.No})` : `StockOut / Add`;
                Dropdown.bind({ id: '#StockOut-StoreId', data: store, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockOut', title: title, action: action, obj: obj });
                Table.add({ id: '#tableStockOutItem', data: obj.VoucherItem, selectPick: true });
                StockOut.sumOfTotalStockOutItem();


            }
        });
    }
    static update(obj) {
        Data.update({
            url: 'StockOut/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalStockOut' });
                    Table.updateById({ id: '#tableStockOut', objId: response.obj.Id, obj: response.obj });
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
                            url: `StockOut/Delete?Id=${id}`,
                            onSuccess: StockOut.deleteOnSuccess
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
         Table.remove({ id: '#tableStockOutItem', value: [index] });
    }
    static deleteScanItem({ id, index }) {
        Table.remove({ id: '#tableStockOutScanItem', value: [index] });
    }
    static print({ obj }) {
        Data.post({
            url: "StockOut/Print",
            data: obj,
            onSuccess: (response) => {
                if (response.status !== Message.Type.success) {
                    Message.error(response);
                    return;
                }

                let reportTitle = 'Stock Out';
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
                    <th>StockOut No.</th>
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
                                <th colspan="7">StockOut Items</th>
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
window.tableStockOutRefNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.RefNo ?? ''}</div>
        <div>${obj.RefDate ? moment(obj.RefDate).format('DD-MMM-YYYY') : ''}</div>
    `;
};
window.tableStockOutSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableStockOutDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableStockOutStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableStockOutCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
}
window.tableStockOutUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${DateTime.dateTime(obj.UpdatedAt)}</div>
    `;
}
window.tableStockOutAction = (value, obj, index) => {
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
window.tableStockOutConAddress = (value, obj, index) => {

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

    // Break into lines (max 2–3 items per line for readability)
    let lines = [];
    for (let i = 0; i < parts.length; i += 2) {
        lines.push(parts.slice(i, i + 2).join(", "));
    }

    return lines.join("<br>");
};
window.tableStockOutActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        StockOut.edit({ id: obj.Id, action: 'Edit' });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        StockOut.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        StockOut.print({ obj: { ListId: [obj.Id] } });
    },
    'click .btn-delete': (e, value, obj, index) => {
        StockOut.delete({ id: obj.Id });
    }
}

//Table StockOut Item
window.tableStockOutItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableStockOutItemDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockOutItem_${index}`, className: 'StockOut-item', data: StockOut.item, value: 'ItemId', text: 'Description', subText: "SubText", initialValue: [obj.ItemId], json: true, parent: '.modal' })}
        <input type="text" id="StockOutItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 StockOut-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
    `;
}
window.tableStockOutItemDescEvent = {
    'change .StockOut-item': (e, value, obj, index) => {
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson?.UnitDesc ?? null;

        obj.Rate = itemJson?.Rate ?? 0;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

        Table.updateByIndex({ id: '#tableStockOutItem', index: index, obj: obj, value: obj.ItemId, event: e });
        StockOut.sumOfTotalStockOutItem();
    },
    'input .StockOut-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableStockOutItem', index: index, obj: obj, value: obj.Remarks, event: e });
    }
}
window.tableStockOutItemRate = (value, obj, index) => {
    return `
        <input type="text" id="StockOutItemRate_${index}" class="form-control form-control-sm text-right StockOut-item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableStockOutItemRateEvent = {
    'input .StockOut-item-rate': (e, value, obj, index) => {
        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableStockOutItem', index: index, obj: obj, value: obj.Rate, event: e });
        StockOut.sumOfTotalStockOutItem();
    }
}
window.tableStockOutItemQty = (value, obj, index) => {
    return `
        <input type="text" id="StockOutItemQty_${index}" class="form-control form-control-sm text-right StockOut-item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableStockOutItemQtyEvent = {
    'input .StockOut-item-qty': (e, value, obj, index) => {
        obj.Qty = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableStockOutItem', index: index, obj: obj, value: obj.Qty, event: e });
        StockOut.sumOfTotalStockOutItem();
    }
}
window.tableStockOutItemAmount = (value, obj, index) => {
    return `
        <input type="text" id="StockOutItemAmt_${index}" class="form-control form-control-sm text-right StockOut-item-amount" value="${obj.Amount}" oninput="this.value = _Number.validate({value: this.value, dp: 2, min: 0, max: 999999999999})">
    `;
}
window.tableStockOutItemAmountEvent = {
    'input .StockOut-item-amount': (e, value, obj, index) => {
        obj.Amount = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        Table.updateByIndex({ id: '#tableStockOutItem', index: index, obj: obj, value: obj.Amount, event: e });
        StockOut.sumOfTotalStockOutItem();
    }
}
window.tableStockOutReasonCodeDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockOutReasonCode_${index}`, className: 'StockOut-item', data: StockOut.reasonCode, value: 'Value', text: 'Description', initialValue: [obj.ReasonCode], json: true, parent: '.modal' })}
       
    `;
}
window.tableStockOutReasonCodeDescEvent = {
    'change .StockOut-item': (e, value, obj, index) => {
        obj.ReasonCode = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj,
            value: obj.ReasonCode,
            event: e
        });
    }
};
window.tableStockOutStoreDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockOutStore_${index}`, className: 'StockOut-item', data: StockOut.store, value: 'Id', text: 'Description', initialValue: [obj.StoreId], json: true, parent: '.modal' })}
       
    `;
}
window.tableStockOutStoreDescEvent = {
    'change .StockOut-item': (e, value, obj, index) => {
        obj.StoreId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj,
            value: obj.StoreId,
            event: e
        });
    }
};
window.tableTaskSerialNo = (value, obj, index) => {
    return `<input type="text" id="StockOutSerialNo_${index}" class="form-control form-control-sm mb-0 StockOut-item" value="${obj.SerialNo}" maxlength="150" />  
    `;
}
window.tableStockOutSerialNoDescEvent = {
    'input .StockOut-item': (e, value, obj, index) => {
        obj.SerialNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj,
            value: obj.SerialNo,
            event: e
        });
    }
};
window.tableTaskBatchNo = (value, obj, index) => {
    return `<input type="text" id="StockOutBatchNo_${index}" class="form-control form-control-sm mb-0 StockOut-item" value="${obj.BatchNo}" maxlength="150" />  
    `;
}
window.tableStockOutBatchNoDescEvent = {
    'input .StockOut-item': (e, value, obj, index) => {
        obj.BatchNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj,
            value: obj.BatchNo,
            event: e
        });
    }
};
//window.tableStockOutExpiryOn = (value, obj, index) => {
//    return `<input type="date" id="StockOutExpiryOn_${index}" class="form-control form-control-sm mb-0 StockOut-item" value="${obj.ExpiryOn ?? ""}"/>`;
//}
window.tableStockOutExpiryOn = (value, obj, index) => {
    let val = obj.ExpiryOn
        ? moment(obj.ExpiryOn, ['DD-MMM-YYYY', 'YYYY-MM-DD']).format('YYYY-MM-DD')
        : '';

    return `
        <input type="date"
            class="form-control form-control-sm mb-0 StockOut-item-expiry"
            value="${val}">
    `;
};
window.tableStockOutExpiryOnEvent = {
    'input .StockOut-item': (e, value, obj, index) => {
        obj.ExpiryOn = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj,
            value: obj.ExpiryOn,
            event: e
        });
    }
};
window.tableStockOutItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableStockOutItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockOut.deleteItem({ id: obj.Id, index: index });
    }
}
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
window.tableStockOutScanItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableStockOutScanItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockOut.deleteScanItem({ id: obj.Id, index: index });
    }
}
window.tableStockItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableStockOutItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockOut.deleteItem({ id: obj.Id, index: index });
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
