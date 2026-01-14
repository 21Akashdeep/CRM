class StockIn {
    static item = [];
    static init() {

        StockIn.getViewOption();

        $('#btnNewEntry').on('click', () => {
            StockIn.newEntry();
        });

        $('#btnSearch').on('click', () => {
            StockIn.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableStockIn', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            StockIn.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableStockIn', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            StockIn.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: 'StockIn' });
                }
            });
        });
        
        $('#StockIn-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let StockInItem = $('#tableStockInItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
            let obj = Data.serializeToObject({ formId: '#formStockIn' });          
            obj.VoucherItem = StockInItem;
            
            if (!obj.Id) {
                StockIn.add(obj);
            }
            else {
                StockIn.update(obj);
            }
        });
    }
    static getViewOption() {
        Data.get({
            url: 'StockIn/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListStockInNo', data: response.data.StockInNo, value: 'Id', text: 'No' })
            }
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'StockIn/GetAddOption', onSuccess: onSuccess });
    }
    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListNo: $('#ListStockInNo').val()
        };
        Data.post({
            url: `StockIn/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static newEntry() {
        StockIn.getAddOption({
            onSuccess: (response) => {
                StockIn.item = response.data.Item;
                let store = response.data.Store;
                            
                Dropdown.bind({ id: '#StockIn-StoreId', data: store, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockIn', title: 'StockIn / Add', action: 'Add' });
                $('#StockIn-RefDate,#StockIn-EwayDate').val('');
            }
        });
    }
    static addStockInItem() {
        let obj = {
            StockInId: 0,
            ItemId: 0,
            VoucherId: 0,
            StoreId: 0,
            ItemDesc: null,
            SerialNo: "",
            BatchNo: "",
            ExpiryOn: null,
            Qty: 0,
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
            Remarks: null
        };
        Table.add({ id: '#tableStockInItem', data: obj, action: 'append', selectPick: true });
        StockIn.sumOfTotalStockInItem();
    }
    static sumOfTotalStockInItem() {
        let StockInItem = $('#tableStockInItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
        let Qty = StockInItem.length == 0 ? 0 : StockInItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);
        let Amount = StockInItem.length == 0 ? 0 : StockInItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="4">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>                    
                </tr>
            </tfoot>
        `;
        $('#tableStockInItem tfoot').remove();
        $('#tableStockInItem').append(tfoot);
    }
    static add(obj) {
        Data.post({
            url: 'StockIn/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalStockIn' });
                    Table.add({ id: '#tableStockIn', data: response.obj, action: 'prepend' });
                    Dropdown.bind({ id: '#ListId', data: response.data.ViewOption.StockIn, value: 'Id', text: 'StockInNo', subText: 'Date' });
                }
            }
        });
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `StockIn/Edit?Id=${id}`,
            onSuccess: (response) => {
                let option = response.data;
                StockIn.item = option.Item;
                let store = response.data.Store;
                let state = response.data.State;
                let obj = response.obj;
                let StoreId = obj.VoucherItem[0].StoreId;
                obj.StoreId = StoreId;
                obj.Id = action == 'Edit' ? obj.Id : null;
                let title = action === 'Edit' ? `StockIn / Edit (Code: ${obj.No})` : `StockIn / Add`;            
                Dropdown.bind({ id: '#StockIn-StoreId', data: store, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockIn', title: title, action: action, obj: obj });
                Table.add({ id: '#tableStockInItem', data: obj.VoucherItem, selectPick: true });
                StockIn.sumOfTotalStockInItem();
               

            }
        });
    }
    static update(obj) {
        Data.update({
            url: 'StockIn/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalStockIn' });
                    Table.updateById({ id: '#tableStockIn', objId: response.obj.Id, obj: response.obj });
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
                            url: `StockIn/Delete?Id=${id}`,
                            onSuccess: StockIn.deleteOnSuccess
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
}
window.tableStockInRefNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.RefNo ?? ''}</div>
        <div>${obj.RefDate ? moment(obj.RefDate).format('DD-MMM-YYYY') : ''}</div>
    `;
};
window.tableStockInSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableStockInDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableStockInStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableStockInCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
}
window.tableStockInUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${DateTime.dateTime(obj.UpdatedAt)}</div>
    `;
}
window.tableStockInAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ?
            `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>` : ``
        }
                ${obj.IsDuplicate ?
            `<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-duplicate" title="Duplicate">
                            <span class="fa fa-copy text-primary-100"></span>&nbsp;&nbsp;Duplicate
                        </a>
                    </li>` : ``
        }
                ${obj.IsDelete ?
            `<li>
                        <a href="#" class="dropdown-item text-danger-100 btn-delete" title="Delete">
                            <span class="fa fa-trash text-danger-100"></span>&nbsp;&nbsp;Delete
                        </a>
                    </li>` : ``
        }
                ${obj.IsEnable ?
            `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-enable" title="Enable">
                            <span class="fa fa-toggle-on text-success-100"></span>&nbsp;&nbsp;Enable
                        </a>
                    </li>` : ``
        }
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableStockInConAddress = (value, obj, index) => {

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
window.tableStockInActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        StockIn.edit({ id: obj.Id, action: 'Edit' });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        StockIn.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        StockIn.print({ id: obj.Id });
    },
    'click .btn-delete': (e, value, obj, index) => {
        StockIn.delete({ id: obj.Id });
    }
}

//Table StockIn Item
window.tableStockInItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableStockInItemDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockInItem_${index}`, className: 'StockIn-item', data: StockIn.item, value: 'Id', text: 'Description', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
        <input type="text" id="StockInItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 StockIn-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
    `;
}
window.tableStockInItemDescEvent = {
    'change .StockIn-item': (e, value, obj, index) => {
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson?.UnitDesc ?? null;

        obj.Rate = itemJson?.Rate ?? 0;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

        Table.updateByIndex({ id: '#tableStockInItem', index: index, obj: obj, value: obj.ItemId, event: e });
        StockIn.sumOfTotalStockInItem();
    },
    'input .StockIn-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableStockInItem', index: index, obj: obj, value: obj.Remarks, event: e });
    }
}
window.tableStockInItemRate = (value, obj, index) => {
    return `
        <input type="text" id="StockInItemRate_${index}" class="form-control form-control-sm text-right StockIn-item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableStockInItemRateEvent = {
    'input .StockIn-item-rate': (e, value, obj, index) => {
        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableStockInItem', index: index, obj: obj, value: obj.Rate, event: e });
        StockIn.sumOfTotalStockInItem();
    }
}
window.tableStockInItemQty = (value, obj, index) => {
    return `
        <input type="text" id="StockInItemQty_${index}" class="form-control form-control-sm text-right StockIn-item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableStockInItemQtyEvent = {
    'input .StockIn-item-qty': (e, value, obj, index) => {
        obj.Qty = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableStockInItem', index: index, obj: obj, value: obj.Qty, event: e });
        StockIn.sumOfTotalStockInItem();
    }
}
window.tableStockInItemAmount = (value, obj, index) => {
    return `
        <input type="text" id="StockInItemAmt_${index}" class="form-control form-control-sm text-right StockIn-item-amount" value="${obj.Amount}" oninput="this.value = _Number.validate({value: this.value, dp: 2, min: 0, max: 999999999999})">
    `;
}
window.tableStockInItemAmountEvent = {
    'input .StockIn-item-amount': (e, value, obj, index) => {
        obj.Amount = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        Table.updateByIndex({ id: '#tableStockInItem', index: index, obj: obj, value: obj.Amount, event: e });
        StockIn.sumOfTotalStockInItem();
    }
}
window.tableStockInReasonCodeDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockInReasonCode_${index}`, className: 'StockIn-item', data: StockIn.reasonCode, value: 'Value', text: 'Description', initialValue: [obj.ReasonCode], json: true, parent: '.modal' })}
       
    `;
}
window.tableStockInReasonCodeDescEvent = {
    'change .StockIn-item': (e, value, obj, index) => {
        obj.ReasonCode = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockInItem',
            index: index,
            obj: obj,
            value: obj.ReasonCode,
            event: e
        });
    }
};
window.tableStockInStoreDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `StockInStore_${index}`, className: 'StockIn-item', data: StockIn.store, value: 'Id', text: 'Description', initialValue: [obj.StoreId], json: true, parent: '.modal' })}
       
    `;
}
window.tableStockInStoreDescEvent = {
    'change .StockIn-item': (e, value, obj, index) => {
        obj.StoreId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        Table.updateByIndex({
            id: '#tableStockInItem',
            index: index,
            obj: obj,
            value: obj.StoreId,
            event: e
        });
    }
};
window.tableTaskSerialNo = (value, obj, index) => {
    return `<input type="text" id="StockInSerialNo_${index}" class="form-control form-control-sm mb-0 StockIn-item" value="${obj.SerialNo}" maxlength="150" />  
    `;
}
window.tableStockInSerialNoDescEvent = {
    'input .StockIn-item': (e, value, obj, index) => {
        obj.SerialNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockInItem',
            index: index,
            obj: obj,
            value: obj.SerialNo,
            event: e
        });
    }
};
window.tableTaskBatchNo = (value, obj, index) => {
    return `<input type="text" id="StockInBatchNo_${index}" class="form-control form-control-sm mb-0 StockIn-item" value="${obj.BatchNo}" maxlength="150" />  
    `;
}
window.tableStockInBatchNoDescEvent = {
    'input .StockIn-item': (e, value, obj, index) => {
        obj.BatchNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockInItem',
            index: index,
            obj: obj,
            value: obj.BatchNo,
            event: e
        });
    }
};
window.tableStockInExpiryOn = (value, obj, index) => {
    return `<input type="date" id="StockInExpiryOn_${index}" class="form-control form-control-sm mb-0 StockIn-item" value="${obj.ExpiryOn ?? ""}"/>`;
}
window.tableStockInExpiryOnEvent = {
    'input .StockIn-item': (e, value, obj, index) => {
        obj.ExpiryOn = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableStockInItem',
            index: index,
            obj: obj,
            value: obj.ExpiryOn,
            event: e
        });
    }
};