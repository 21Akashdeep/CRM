class StockOut {
    static item = [];
    static itemWithSerialNo = [];
    static unit = [];
    static conFator;
    static mode = 'Add';
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
            if (e.key != 'Enter') return;
            let item = StockOut.itemWithSerialNo.find(x => x.SerialNo == $('#StockOutScan-ItemSerialNo').val());
            if (!item) {
                Message.error({ statusText: `Invalid Serial No. (${$('#StockOutScan-ItemSerialNo').val()})` });
                return;
            }
            StockOut.addStockOutItem({
                itemId: item.ItemId,
                itemDesc: item.ItemDesc,
                expiryOn: item.ExpiryOn,
                serialNo: item.SerialNo,
                unitDesc: item.UnitDesc,
                qty: 1,
                unitId: 1,
                baseQty: 1,
                conversionFactor: 1,
                isScanned: true,
                callback: (obj) => {
                    Table.add({ id: '#tableStockOutScanItem', data: obj, action: 'prepend' });
                    $('#StockOutScan-ItemSerialNo').val('');
                }
            });
        });
        $('#btnScanItemAdd').on('click', () => {
            let scanItems = $('#tableStockOutScanItem').bootstrapTable('getData');
            scanItems.forEach(item => {
                item.UnitId = 1;
            });
            if (scanItems.length === 0) {
                Message.error({ statusText: 'No scanned items found' });
                return;
            }
            Table.add({ id: '#tableStockOutItem', data: scanItems, action: 'append', selectPick: true });
            StockOut.sumOfTotalStockOutItem();

            Modal.close({ id: '#modalStockOutItemScan' });
        });
        $('#StockOut-StockType').on('change', () => {

            let store = $('#StockOut-StoreId').val() || 0;
            let stockType = [$('#StockOut-StockType').val()] || 0;
            if (store == 0 || stockType == 0) {
                Message.error({ statusText: 'Store is not selected!!!' });
                return;
            }
            let obj = {
                ListStoreId: [$('#StockOut-StoreId').val()],
                ListStockType: stockType
            };
            Data.post({
                url: 'StockOut/GetStockItem',
                data: obj,
                onSuccess: (response) => {
                    StockOut.item = response.data;
                }
            });
        });
        Party.initAdd();
        Party.addOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalParty' });
            StockOut.getAddOption({
                onSuccess: (response) => {
                    Dropdown.bind({ id: '#StockOut-PartyId', data: response.data.Party, value: 'Id', text: 'Description', json: true, initialValue: [obj.Id] });
                    Field.triggerOnChange('#StockOut-PartyId');
                }
            });
        }
        Party.updateOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalParty' });
            setTimeout(() => {
                StockOut.getAddOption({
                    onSuccess: (response) => {
                        Dropdown.bind({ id: '#StockOut-PartyId', data: response.data.Party, value: 'Id', text: 'Description', json: true, initialValue: [obj.Id] });
                        Field.triggerOnChange('#StockOut-PartyId');
                    }
                });
            }, 500);
        }

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
            let ZeroQtyItem = StockOutItem.filter(x => x.Qty == 0);
            if (ZeroQtyItem.length != 0) {
                Message.error({ statusText: 'Qty Zero Input in Any Item!!!' });
                return;
            }
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
            ListNo: $('#ListStockOutNo').val(),
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
                StockOut.mode = 'Add';
                StockOut.item = response.data.Item;
                let store = response.data.Store;
                Dropdown.bind({ id: '#StockOut-StoreId', data: store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#StockOut-StockType', data: response.data.StockType, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#StockOut-PartyId', data: response.data.Party, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockOut', title: 'StockOut / Add', action: 'Add' });
                $('#StockOut-RefDate,#StockOut-EwayDate').val('');
            }
        });
    }
    static scanItem() {
        if (Field.isNullOrEmpty($('#StockOut-StoreId').val())) {
            Message.error({ statusText: 'Store is not selected!!!' });
            return;
        }
        let obj = {
            ListStoreId: [$('#StockOut-StoreId').val()],
        };
        Data.post({
            url: 'StockOut/getStockItemWithSerialNo',
            data: obj,
            onSuccess: (response) => {
                Modal.open({ id: '#modalStockOutItemScan', title: 'StockOut / Scan Item' });
                StockOut.itemWithSerialNo = response.data;
            }
        });
    }
    static getItemDetails() {
        let obj = {
            ListStoreId: [$('#StockOut-StoreId').val()],
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            Todate: DateTime.json($('#DateRange').val().split('|')[1])

        };
        Data.post({
            url: 'StockOut/getStockItem',
            data: obj,
            onSuccess: (response) => {
                if (!response.data || response.data.length === 0)
                    return;
                StockOut.item = response.data;

            }
        });
    }
    static addStockOutItem({ itemId = 0, itemDesc = null, serialNo = "", expiryOn = null, qty = 0, conversionFactor, unitDesc = null, unitId = 0, isScanned = false, isReturnable = false, callback } = {}) {

        let store = $('#StockOut-StoreId').val() || 0;
        let stockType = $('#StockOut-StockType').val() || 0;
        if (store == 0 || stockType == 0) {
            Message.error({ statusText: 'Select Both Store And StockType!!!' });
            return;       
        }

        let obj = {
            Id: 0,
            ItemId: itemId,
            VoucherId: 0,
            StoreId: 0,
            ItemDesc: itemDesc,
            UnitId: unitId,
            SerialNo: serialNo,
            BatchNo: "",
            ExpiryOn: expiryOn,
            Qty: qty,
            ConversionFactor: conversionFactor,
            BaseQty:qty,
            Rate: 0,
            UnitDesc: unitDesc,
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
            IsScanned: isScanned,
            IsReturnable :isReturnable
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
        let Qty = StockOutItem.length == 0 ? 0 : StockOutItem.map(x => Math.abs(parseFloat(x.Qty) || 0)).reduce((s, v) => s + v, 0);
        let Amount = StockOutItem.length == 0 ? 0 : StockOutItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="6">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>
                     <th class="text-right"></th>
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
    static getUnit(id, callback) {
        Data.get({
            url: `StockOut/getUnit?Id=${id}`,
            onSuccess: (response) => {
                callback(response.data);
            }
        });
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `StockOut/Edit?Id=${id}`,
            onSuccess: (response) => {               
                let option = response.data;
                Dropdown.bind({ id: '#StockOut-StoreId', data: option.AddOption.Store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#StockOut-PartyId', data: option.AddOption.Party, value: 'Id', text: 'Description' });
                StockOut.item = option.StockItem;               
                let obj = option.Voucher;
                StockOut.mode = action;
                obj.Id = action == 'Edit' ? obj.Id : null;
                let title = action === 'Edit' ? `StockOut / Edit (Code: ${obj.No})` : `StockOut / Add`;
                Modal.open({ id: '#modalStockOut', title: title, action: action, obj: obj });
                Table.add({ id: '#tableStockOutItem', data: obj.VoucherItem, selectPick: true });
                StockOut.getItemDetails();
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
                <td>${obj.ChallanNo || '-'}</td>                
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
                                    <td class="text-right">${Math.abs(row.Qty)}</td>
                                </tr>
                            `).join('')}
                            <tr>
                                <th colspan="4" class="text-right">Total</th>
                                <th class="text-right">
                                    ${
                                 (() => {
                            const uniqueUnits = Data.unique({ data: items, field: 'UnitId' });
                        return uniqueUnits.length === 1
                            ? items.reduce((s, x) => s + Number(Math.abs(x.Qty) || 0), 0) : '';
                    })() }
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
window.tableStockOutChallanNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.ChallanNo ?? ''}</div>
        <div>${obj.ChallanDate ? moment(obj.ChallanDate).format('DD-MMM-YYYY') : ''}</div>
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
    setTimeout(() => {
        if (!obj.UnitList && obj.ItemId) {
            StockOut.getUnit(obj.ItemId, (unitList) => {
                obj.UnitList = unitList;
                Table.updateByIndex({
                    id: '#tableStockOutItem',
                    index: index,
                    obj: obj
                });
            });
        }
    }, 0);
    return `
        ${Dropdown.html({ id: `StockOutItem_${index}`, className: 'StockOut-item', data: StockOut.item, value: 'ItemId', text: 'ItemDesc', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
        <input type="text" id="StockOutItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 StockOut-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
    `;
}
window.tableStockOutItemDescEvent = {
    'change .StockOut-item': (e, value, obj, index) => {
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        //Set Unit & Rate
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson?.UnitDesc ?? null;
        obj.UnitId = itemJson?.UnitId ?? 0;
        obj.Rate = itemJson?.Rate ?? 0;
        //GetUnit
        StockOut.getUnit(obj.ItemId, (unitList) => {

            obj.UnitList = unitList;
            obj.UnitId = unitList?.[0]?.Id || 0;
            obj.UnitDesc = unitList?.[0]?.UnitDesc || null;

            Table.updateByIndex({
                id: '#tableStockOutItem',
                index: index,
                obj: obj,
                event: e
            });

        });
        // Set Serial No. & BalQty
        let item = StockOut.item.find(x => x.ItemId == obj.ItemId);
        obj.BalQty = item ? item.Qty : 0;
        //Sum of Amount
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        //Update Row Data
        Table.updateByIndex({ id: '#tableStockOutItem', index: index, obj: obj, value: obj.ItemId, event: e });
        StockOut.sumOfTotalStockOutItem();
    },
    'input .StockOut-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableStockOutItem', index: index, obj: obj, value: obj.Remarks, event: e });
    }
}
window.tableStockOutItemUnitDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({
        id: `StockOutUnit_${index}`,
        className: 'stockout-unit',
        data: obj.UnitList || [],
        value: 'UnitId',
        text: 'UnitDesc',
        initialValue: [obj.UnitId],
        json: true,
        parent: '.modal'
    })}
    `;
}
window.tableStockOutItemUnitDescEvent = {
    'change .stockout-unit': (e, value, obj, index) => {
        obj.UnitId = parseInt(e.currentTarget.value) || 0;
        let unitJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = unitJson?.UnitDesc || null;
        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj,
            event: e
        });

        StockOut.sumOfTotalStockOutItem();
    }
}
window.tableStockOutItemExpiryOn = (value, obj, index) => {
    return obj.ExpiryOn ? moment(obj.ExpiryOn).format("DD-MMM-YYYY") : "-";
}
window.tableStockOutItemQty = (value, obj, index) => {
    return `
        <div class="input-group">            
            <input type="text" id="StockOutItemQty_${index}" class="form-control form-control-sm text-end stockout-item-qty" value="${Math.abs(obj.Qty)}" 
            oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})"/>
            <span class="input-group-text fw-bold">${obj.UnitDesc ?? 'NA'}</span>
        </div>
    `;
}
window.tableStockOutItemQtyEvent = {
    'input .stockout-item-qty': (e, value, obj, index) => {

        let qty = parseFloat(e.currentTarget.value || 0);
        let balQty = parseFloat(obj.BalQty || 0);

        let unitId = $(`#StockOutUnit_${index}`).val();
        unitId = unitId ? unitId : 1;
        let selectedUnit = obj.UnitList?.find(x => x.UnitId == unitId);
        selectedUnit = selectedUnit ? selectedUnit : [];
        let cf = selectedUnit?.ConversionFactor || 1;
        obj.BaseQty = qty * cf;
        obj.ConversionFactor = cf;
        let valuePerUnit = selectedUnit.ValuePerUnit ? selectedUnit.ValuePerUnit:1;
        let avQty = balQty * valuePerUnit;
        // minus qty check
        if (qty < 0) {
            Message.error({ statusText: 'Qty cannot be negative' });
            e.currentTarget.value = obj.Qty || 0;
            return;
        }
        // available qty check
        if (qty > avQty) {
            Message.error({ statusText: `Qty cannot be greater than available qty (${avQty})` });
            e.currentTarget.value = obj.Qty || 0;
            return;
        }
        obj.Qty = qty;
        obj.UnitId = unitId;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj,
            value: obj.Qty,
            event: e
        });
        StockOut.sumOfTotalStockOutItem();
    }
}
window.tableStockOutIsReturnable = (value, obj, index) => {
    let isYes = value === true || value === "true" || value === 1;
    if (StockOut.mode === 'Edit') {
        return `<span>${obj.IsReturnable}</span>`;
    }
    return `
        <select class="form-control form-control-sm table-isreturnable-select">
            <option value="false" ${obj.IsReturnable == 0 ? "selected" : ""}>No</option>
            <option value="true" ${obj.IsReturnable == 1 ? "selected" : ""}>Yes</option>
        </select>`;
};
window.tableStockOutIsReturnableEvent = {
    'change .table-isreturnable-select': (e, value, obj, index) => {
        if (StockOut.mode === 'Edit') return;
        let newValue = e.currentTarget.value === "true";
        obj.IsReturnable = newValue;
        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj
        });
    }
};
window.tableStockOutIsReturned = (value, obj, index) => {
    let isYes = value === true || value === "true" || value === 1;
    if (StockOut.mode === 'Add' || obj.IsReturnable == false) {
        return `<span>-</span>`;
    }
    return `
        <select class="form-control form-control-sm table-isreturned-select">
            <option value="false" ${obj.IsReturned == 0 ? "selected" : ""}>No</option>
            <option value="true" ${obj.IsReturned == 1 ? "selected" : ""}>Yes</option>
        </select>`;
};
window.tableStockOutIsReturnedEvent = {
    'change .table-isreturned-select': (e, value, obj, index) => {
        if (StockOut.mode === 'Add') return;
        let newValue = e.currentTarget.value === "true";
        obj.IsReturned = newValue;
        Table.updateByIndex({
            id: '#tableStockOutItem',
            index: index,
            obj: obj
        });
    }
};
//StockOutScanTable
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
window.tableStockOutScanExpiryOn = (value, obj, index) => {
    let val = obj.ExpiryOn
        ? moment(obj.ExpiryOn, ['DD-MMM-YYYY', 'YYYY-MM-DD']).format('YYYY-MM-DD')
        : '';

    return `
        <input type="date"
            class="form-control form-control-sm mb-0 StockOut-item-expiry"
            value="${val}">
    `;
};
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
