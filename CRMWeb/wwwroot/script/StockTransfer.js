class StockTransfer {
    static item = [];
    static init() {

        StockTransfer.getViewOption();

        $('#btnNewEntry').on('click', () => {
            StockTransfer.newEntry();
        });

        $('#btnSearch').on('click', () => {
            StockTransfer.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableStockTransfer', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            StockTransfer.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableStockTransfer', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            StockTransfer.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: 'StockTransfer' });
                }
            });
        });
        $('#StockTransfer-Scan').on('keydown', (e) => {
            if (e.key == "Enter") {
                try {
                    let obj = JSON.parse($('#StockTransfer-Scan').val());
                    StockTransfer.addStockTransferItem({ ItemId: obj.ItemId, SerialNo: obj.SerialNo, BatchNo: obj.BatchNo, Qty: 1 });
                }
                catch (ex) {
                    Message.error({ statusText: "Scanned QR Code is Invalid." });
                }
                $('#StockTransfer-Scan').val('');
            }
        });
        $('#StockTransferScan-ItemSerialNo').on('keydown', (e) => {

            if (e.key == 'Enter') {

                if (!Field.isMandatory({ class: '.scan-required' })) {
                    return;
                }

                let serialNo = $('#StockTransferScan-ItemSerialNo').val();

                let countStockTransferItem =
                    $('#tableStockTransferItem')
                        .bootstrapTable('getData')
                        .filter(x => x.SerialNo == serialNo).length;

                let countScanItem =
                    $('#tableStockTransferScanItem')
                        .bootstrapTable('getData')
                        .filter(x => x.SerialNo == serialNo).length;

                if (countStockTransferItem > 0 || countScanItem > 0) {
                    Message.error({
                        statusText: `Serial No. ${serialNo} already added in Scan List or Item List.`
                    });
                    return;
                }

                StockTransfer.addStockTransferItem({
                    itemId: $('#StockTransferScan-ItemId').val(),
                    itemDesc: $('#StockTransferScan-ItemId option:selected').text(),
                    expiryOn: $('#StockTransferScan-ItemExpiryOn').val(),
                    serialNo: serialNo,
                    qty: 1,
                    isScanned: true,
                    callback: (obj) => {
                        Table.add({
                            id: '#tableStockTransferScanItem',
                            data: obj,
                            action: 'prepend'
                        });
                        $('#StockTransferScan-ItemSerialNo').val('');
                    }
                });
            }
        });
        $('#btnScanItemAdd').on('click', () => {

            let scanItems =
                $('#tableStockTransferScanItem').bootstrapTable('getData');

            if (scanItems.length === 0) {
                Message.error({ statusText: 'No scanned items found' });
                return;
            }

            Table.add({
                id: '#tableStockTransferItem',
                data: scanItems,
                action: 'append',
                selectPick: true
            });

            StockTransfer.sumOfTotalStockTransferItem();

            Modal.close({ id: '#modalStockTransferItemScan' });
        });
        $('#StockTransfer-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let StockTransferItem = $('#tableStockTransferItem').bootstrapTable('getData').filter(x => x.ItemId > 0)
                .map(x => {
                    if (x.ExpiryOn) {
                        x.ExpiryOn = moment(x.ExpiryOn).format('YYYY-MM-DD');
                    }
                    return x;
                });
            let obj = Data.serializeToObject({ formId: '#formStockTransfer' });
            obj.VoucherItem = StockTransferItem;

            if (!obj.Id) {
                StockTransfer.add(obj);
            }
            else {
                StockTransfer.update(obj);
            }
        });
    }
    static getViewOption() {
        Data.get({
            url: 'StockTransfer/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListStockTransferNo', data: response.data.StockTransferNo, value: 'Id', text: 'No' })
            }
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'StockTransfer/GetAddOption', onSuccess: onSuccess });
    }
    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListNo: $('#ListStockTransferNo').val()
        };
        Data.post({
            url: `StockTransfer/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static newEntry() {
        StockTransfer.getAddOption({
            onSuccess: (response) => {
                StockTransfer.item = response.data.Item;
                let store = response.data.Store;

                Dropdown.bind({ id: '#FromStoreId', data: store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#ToStoreId', data: store, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockTransfer', title: 'StockTransfer / Add', action: 'Add' });
                $('#StockTransfer-RefDate,#StockTransfer-EwayDate').val('');
            }
        });
    }
    static scanner() {
        Modal.open({
            id: '#modalStockTransferItemScan',
            title: 'StockTransfer / Scan Item'
        });

        Dropdown.bind({
            id: '#StockTransferScan-ItemId',
            data: StockTransfer.item,
            value: 'Id',
            text: 'Description'
        });

        $('#StockTransferScan-ItemSerialNo').val('');
    }
    static addStockTransferItem({ itemId = 0, itemDesc = null, serialNo = "", expiryOn = null, qty = 0, isScanned = false, callback } = {}) {
        let obj = {
            Id: 0,
            StockTransferId: 0,
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
            Table.add({
                id: '#tableStockTransferItem', data: obj, action: 'append', selectPick: true
            });
            StockTransfer.sumOfTotalStockTransferItem();
        }
        else {
            callback(obj);
        }
    }
    static sumOfTotalStockTransferItem() {
        let StockTransferItem = $('#tableStockTransferItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
        let Qty = StockTransferItem.length == 0 ? 0 : StockTransferItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);
        let Amount = StockTransferItem.length == 0 ? 0 : StockTransferItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="4">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>                    
                </tr>
            </tfoot>
        `;
        $('#tableStockTransferItem tfoot').remove();
        $('#tableStockTransferItem').append(tfoot);
    }
    static add(obj) {
        Data.post({
            url: 'StockTransfer/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalStockTransfer' });
                    Table.add({ id: '#tableStockTransfer', data: response.obj, action: 'prepend' });
                    Dropdown.bind({ id: '#ListId', data: response.data.ViewOption.StockTransfer, value: 'Id', text: 'StockTransferNo', subText: 'Date' });
                }
            }
        });
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `StockTransfer/Edit?Id=${id}`,
            onSuccess: (response) => {
                let option = response.data;
                StockTransfer.item = option.Item;
                let store = response.data.Store;
                let obj = response.obj;
                let FromStoreId = obj.VoucherItem.filter(x => x.Qty < 0)[0].StoreId;
                let ToStoreId = obj.VoucherItem.filter(x => x.Qty >= 0)[0].StoreId;
                let voucherItem = obj.VoucherItem.filter(x => x.Qty >= 0);
                obj.Id = action == 'Edit' ? obj.Id : null;
                let title = action === 'Edit' ? `StockTransfer / Edit (Code: ${obj.No})` : `StockTransfer / Add`;
                Dropdown.bind({ id: '#FromStoreId', data: store, value: 'Id', text: 'Description'});
                Dropdown.bind({ id: '#ToStoreId', data: store, value: 'Id', text: 'Description'});
                Modal.open({ id: '#modalStockTransfer', title: title, action: action, obj: obj });
                Table.add({ id: '#tableStockTransferItem', data:voucherItem, selectPick: true });
                StockTransfer.sumOfTotalStockTransferItem();


            }
        });
    }
    static update(obj) {
        Data.update({
            url: 'StockTransfer/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalStockTransfer' });
                    Table.updateById({ id: '#tableStockTransfer', objId: response.obj.Id, obj: response.obj });
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
                            url: `StockTransfer/Delete?Id=${id}`,
                            onSuccess: StockTransfer.deleteOnSuccess
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
        if (index >= 0 && id == 0) {
            Table.remove({ id: '#tableStockTransferItem', value: [index] });
            return;
        }
        Message.confirm({
            msg: "Do you want to delete?",
            confirmButtonText: "Delete",
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({
                    url: `StockTransfer/DeleteItem?Id=${id}`,
                    onSuccess: StockTransfer.deleteItemOnSuccess
                });
            }
        });
    }
    static deleteItemOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableTask", obj: response.data.obj.VoucherItem });
            Table.add({ id: '#tableStockTransferItem', data: data.obj.VoucherItem, selectPick: true })
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }
}

window.tableStockTransferRefNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.RefNo ?? ''}</div>
        <div>${obj.RefDate ? moment(obj.RefDate).format('DD-MMM-YYYY') : ''}</div>
    `;
};
window.tableStockTransferSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableStockTransferDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableStockTransferStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableStockTransferCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
}
window.tableStockTransferUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${DateTime.dateTime(obj.UpdatedAt)}</div>
    `;
}

window.tableStockTransferAction = (value, obj, index) => {
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

    return `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
            ${actionBtn.join('')}
            </ul>
        </div>
    `;
}
window.tableStockTransferConAddress = (value, obj, index) => {

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
window.tableStockTransferActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        StockTransfer.edit({ id: obj.Id, action: 'Edit' });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        StockTransfer.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        StockTransfer.print({ id: obj.Id });
    },
    'click .btn-delete': (e, value, obj, index) => {
        StockTransfer.delete({ id: obj.Id });
    }
}

//Table StockTransfer Item
window.tableStockTransferItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableStockTransferItemDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockTransferItem_${index}`, className: 'StockTransfer-item', data: StockTransfer.item, value: 'Id', text: 'Description', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
        <input type="text" id="StockTransferItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 StockTransfer-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
    `;
}
window.tableStockTransferItemDescEvent = {
    'change .StockTransfer-item': (e, value, obj, index) => {
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson?.UnitDesc ?? null;

        obj.Rate = itemJson?.Rate ?? 0;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

        Table.updateByIndex({ id: '#tableStockTransferItem', index: index, obj: obj, value: obj.ItemId, event: e });
        StockTransfer.sumOfTotalStockTransferItem();
    },
    'input .StockTransfer-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableStockTransferItem', index: index, obj: obj, value: obj.Remarks, event: e });
    }
}
window.tableStockTransferItemRate = (value, obj, index) => {
    return `
        <input type="text" id="StockTransferItemRate_${index}" class="form-control form-control-sm text-right StockTransfer-item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableStockTransferItemRateEvent = {
    'input .StockTransfer-item-rate': (e, value, obj, index) => {
        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableStockTransferItem', index: index, obj: obj, value: obj.Rate, event: e });
        StockTransfer.sumOfTotalStockTransferItem();
    }
}
window.tableStockTransferItemQty = (value, obj, index) => {
    return `
        <input type="text" id="StockTransferItemQty_${index}" class="form-control form-control-sm text-right StockTransfer-item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableStockTransferItemQtyEvent = {
    'input .StockTransfer-item-qty': (e, value, obj, index) => {
        obj.Qty = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableStockTransferItem', index: index, obj: obj, value: obj.Qty, event: e });
        StockTransfer.sumOfTotalStockTransferItem();
    }
}
window.tableStockTransferItemAmount = (value, obj, index) => {
    return `
        <input type="text" id="StockTransferItemAmt_${index}" class="form-control form-control-sm text-right StockTransfer-item-amount" value="${obj.Amount}" oninput="this.value = _Number.validate({value: this.value, dp: 2, min: 0, max: 999999999999})">
    `;
}
window.tableStockTransferItemAmountEvent = {
    'input .StockTransfer-item-amount': (e, value, obj, index) => {
        obj.Amount = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        Table.updateByIndex({ id: '#tableStockTransferItem', index: index, obj: obj, value: obj.Amount, event: e });
        StockTransfer.sumOfTotalStockTransferItem();
    }
}
window.tableStockTransferReasonCodeDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockTransferReasonCode_${index}`, className: 'StockTransfer-item', data: StockTransfer.reasonCode, value: 'Value', text: 'Description', initialValue: [obj.ReasonCode], json: true, parent: '.modal' })}
       
    `;
}
window.tableStockTransferReasonCodeDescEvent = {
    'change .StockTransfer-item': (e, value, obj, index) => {
        obj.ReasonCode = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockTransferItem',
            index: index,
            obj: obj,
            value: obj.ReasonCode,
            event: e
        });
    }
};
window.tableStockTransferStoreDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockTransferStore_${index}`, className: 'StockTransfer-item', data: StockTransfer.store, value: 'Id', text: 'Description', initialValue: [obj.StoreId], json: true, parent: '.modal' })}
       
    `;
}
window.tableStockTransferStoreDescEvent = {
    'change .StockTransfer-item': (e, value, obj, index) => {
        obj.StoreId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        Table.updateByIndex({
            id: '#tableStockTransferItem',
            index: index,
            obj: obj,
            value: obj.StoreId,
            event: e
        });
    }
};
window.tableTaskSerialNo = (value, obj, index) => {
    return `<input type="text" id="StockTransferSerialNo_${index}" class="form-control form-control-sm mb-0 StockTransfer-item" value="${obj.SerialNo}" maxlength="150" />  
    `;
}
window.tableStockTransferSerialNoDescEvent = {
    'input .StockTransfer-item': (e, value, obj, index) => {
        obj.SerialNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockTransferItem',
            index: index,
            obj: obj,
            value: obj.SerialNo,
            event: e
        });
    }
};
window.tableStockTransferBatchNoDescEvent = {
    'input .StockTransfer-item': (e, value, obj, index) => {
        obj.BatchNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockTransferItem',
            index: index,
            obj: obj,
            value: obj.BatchNo,
            event: e
        });
    }
};
window.tableStockTransferExpiryOn = (value, obj, index) => {
    return `<input type="date" id="StockTransferExpiryOn_${index}" class="form-control form-control-sm mb-0 StockTransfer-item" value="${obj.ExpiryOn ?? ""}"/>`;
}
window.tableStockTransferExpiryOnEvent = {
    'input .StockTransfer-item': (e, value, obj, index) => {
        obj.ExpiryOn = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockTransferItem',
            index: index,
            obj: obj,
            value: obj.ExpiryOn,
            event: e
        });
    }
};
window.tableStockTransferItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableStockTransferItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockTransfer.deleteItem({ id: obj.Id, index: index });
    }
}
window.tableStockTransferScanItemDesc = (v, obj) => {
    let item = StockTransfer.item.find(x => x.Id === obj.ItemId);
    return item?.Description ?? '';
};
window.tableStockTransferScanExpiry = (v, obj) => {
    return obj.ExpiryOn
        ? moment(obj.ExpiryOn).format('DD-MMM-YYYY')
        : '';
};
window.tableStockTransferScanQty = (v, obj) => obj.Qty;
window.tableStockTransferScanSerial = (v, obj) => obj.SerialNo;
window.tableStockTransferScanItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `
            <button type="button"
                class="btn btn-sm btn-danger rounded-5 btn-delete">
                <span class="fa fa-trash"></span>
            </button>`;
};
window.tableStockTransferScanItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        Table.remove({
            id: '#tableStockTransferScanItem',
            value: [index]
        });
    }
};
window.tableStockTransferExpiryOn = (value, obj) => {

    let val = obj.ExpiryOn
        ? moment(obj.ExpiryOn, ['DD-MMM-YYYY', 'YYYY-MM-DD']).format('YYYY-MM-DD')
        : '';

    return `
        <input type="date"
            class="form-control form-control-sm mb-0 StockTransfer-item-expiry"
            value="${val}">
    `;
};
window.tableStockTransferExpiryOnEvent = {
    'input .StockTransfer-item-expiry': (e, value, obj, index) => {
        obj.ExpiryOn = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockTransferItem',
            index,
            obj
        });
    }
};
window.tableStockTransferItemAction = (value, obj, index) => {
    
        return `
            <button type="button"
                class="btn btn-sm btn-danger rounded-5 btn-delete">
                <span class="fa fa-trash"></span>
            </button>`
};
window.tableStockTransferItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockTransfer.deleteItem({ id: obj.Id, index: index });
    }
};
