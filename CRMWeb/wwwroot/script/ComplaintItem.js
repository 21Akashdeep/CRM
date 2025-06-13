class ComplaintItem {
    static Item = [];
    static init() {     
        ComplaintItem.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListComplaintId', data: response.data.Complaint, value: 'Id', text: 'Code', subText: "SubText" });
                Dropdown.bind({ id: '#ListCustomerId', data: response.data.Customer, value: 'Id', text: 'Description', subText: 'Address' });
            }
        });
        $('#btnSearch').on('click', () => {
            ComplaintItem.get({
                method:'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableComplaintItem', data: response.data });
                }
            });
        });   
        $('#btnNewEntry').on('click', () => {
            ComplaintItem.newEntry();
        });
        $('#ComplaintId').on('change', () => {
            let obj = Dropdown.itemJson({ id: '#ComplaintId' });
            Table.empty({ selector: '#tableItem' });
            if (obj) {
                $('#CustomerDesc').val(obj.CustomerDesc);
                $('#CustomerAddress').val(obj.CustomerAddress);
                $('#Problem').val(obj.Problem);
                if (obj.IsItemAdded) {
                    ComplaintItem.get({
                        method: 'Get',
                        onSuccess: (response) => {
                            Table.add({ id: '#tableItem', data: response.data, selectPick: true });
                            ComplaintItem.sumOfTotalItem();
                        }
                    });
                }
            }
        });
        $('#btnSave').on('click', () => {
            let list = $('#tableItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
            if (list.length == 0) {
                Message.error({ statusText: 'Please select atleat one item' });
                return;
            }
            ComplaintItem.add(list);
        });
    }
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'ComplaintItem/GetViewOption', onSuccess: onSuccess });
    }
    static get({ method, onSuccess }) {
        let obj = {
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListStatus: $('#ListStatus').val(),
            ListComplaintId: $('#ListComplaintId').val(),
            ListCustomerId: $('#ListCustomerId').val(),
        };
        Data.post({ url: `ComplaintItem/${method}`, data: obj, onSuccess: onSuccess });
    }
    static newEntry() {
        Data.get({
            url: 'ComplaintItem/GetAddOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ComplaintId', data: response.data.Complaint, value: 'Id', text: 'Code', json: true });
                ComplaintItem.Item = response.data.Item;                
                Modal.open({ id: '#modalComplaintItem', title: 'Complaint Item / Add or Edit', action: 'Add' });
            }
        });
    }
    static addItem() {
        let objComplaint = Dropdown.itemJson({ id: '#ComplaintId' });
        if (objComplaint) {
            let obj = {
                Id: 0,
                ComplaintId: objComplaint.Id,
                ItemId: 0,                
                UnitDesc: null,
                SerialNo: null,
                Rate: 0,
                UnitDesc: null,
                Qty: 0,
                Amount: 0,
                IsDelete: false
            };
            Table.add({ id: '#tableItem', data: obj, action: 'append', selectPick: true});
        }        
        else {
            Message.error({ statusText: 'Please select complaint no.' });
        }
        ComplaintItem.sumOfTotalItem();
    }
    static sumOfTotalItem() {
        let Item = $('#tableItem').bootstrapTable('getData');
        let Qty = Item.length == 0 ? 0 : Item.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v);
        let Amount = Item.length == 0 ? 0 : Item.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v);
        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="4">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>
                    <th class="text-right">${_Number.format({ num: Amount, dp: 3, currency: 'INR' })}</th>
                    <th class="text-right"></th>
                </tr>
            </tfoot>
        `;
        $('#tableItem tfoot').remove();
        $('#tableItem').append(tfoot);
    }
    static add(list) {
        Data.post({
            url: 'ComplaintItem/Add',
            data: list,
            onSuccess: (response) => {
                Message.success(response)
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalComplaintItem' });
                    Table.add({ id: '#tableComplaintItem', data: response.data, action: 'prepend' });
                }
            }
        });
    }
    static delete({ obj, index = null }) {
        if (obj.IsDelete) {
            Message.confirm({
                msg: 'Do you want to delete??',
                confirmButtonText: "Delete",
                denyButtonText: "Don't Delete",
                data: obj,
                onConfirm: (obj) => {
                    Data.delete({
                        url: `ComplaintItem/Delete?Id=${obj.Id}`,
                        onSuccess: (response) => {
                            Message.show(response);
                            if (response.status == Message.Type.success) {
                                if (response.obj) {
                                    Table.updateById({ id: '#tableComplaintItem', objId: response.obj.Id, obj: response.obj });
                                }                                
                                if (index != null) {
                                    Table.remove({ id: '#tableItem', value: [index] });
                                }                                
                            }
                        }
                    });
                }
            });            
        }
        else {
            Table.remove({ id: '#tableItem', value: [index] });
        }
    }
}


//Table Compaint Item
window.tableComplaintItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableComplaintItemComplaintDate = (value, obj, index) => {
    return moment(obj.ComplaintDate).format('DD-MMM-YYYY');
}
window.tableComplaintItemCustomerDesc = (value, obj, index) => {
    return `
        <div class="fw-bold">${obj.CustomerDesc.match(/.{1,40}/g).join('<br>')}</div>
        <div>${obj.CustomerAddress.match(/.{1,40}/g).join('<br>')}</div>
    `;
}
window.tableComplaintItemProblem = (value, obj, index) => {
    return `<div>${obj.Problem.match(/.{1,50}/g).join('<br>')}</div>`;
}
window.tableComplaintItemDesc = (value, obj, index) => {
    return `<div>${obj.ItemDesc.match(/.{1,50}/g).join('<br>')}</div>`;
}
window.tableComplaintItemRate = (value, obj, index) => {
    return _Number.format({ num: obj.Rate, dp: 3, currency: 'INR' });
}
window.tableComplaintItemQty = (value, obj, index) => {
    return _Number.format({ num: obj.Rate, dp: 3 });
}
window.tableComplaintItemAmount = (value, obj, index) => {
    return _Number.format({ num: obj.Amount, dp: 2, currency: 'INR' });
}
window.tableComplaintItemCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
}
window.tableComplaintItemStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableComplaintItemAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ? `
                    <li>
                        <a href="#" class="dropdown-item text-success btn-edit" title="View / Edit">
                            <span class="fa fa-edit"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>` : ``
                }                
                ${obj.IsDelete ? `
                    <li>
                        <a href="#" class="dropdown-item text-danger btn-delete" title="Delete">
                            <span class="fa fa-trash"></span>&nbsp;&nbsp;Delete
                        </a>
                    </li>` : ``
                }                
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableComplaintItemActionEvent = {
    'click .btn-delete': (e, value, obj, index) => {
        ComplaintItem.delete({ obj: obj });
    }
}

//Table Item
window.tableItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableItemDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `Item_${index}`, className: 'item', data: ComplaintItem.Item, value: 'ItemId', text: 'ItemDesc', initialValue: [obj.ItemId], json: true, parent:'.modal' })}
        <input type="text" id="SerialNo_${index}" class="form-control form-control-sm mb-0 mt-1" maxlength="20">
    `;
}
window.tableItemDescEvent = {
    'change .item': (e, value, obj, index) => {
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson ? itemJson.UnitDesc : null;
        Table.updateByIndex({ id: '#tableItem', index: index, obj: obj, value: obj.ItemId, event: e });
        ComplaintItem.sumOfTotalItem();
    }
}
window.tableItemRate = (value, obj, index) => {    
    return `
        <input type="text" id="Rate_${index}" class="form-control form-control-sm text-right item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableItemRateEvent = {
    'input .item-rate': (e, value, obj, index) => {
        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableItem', index: index, obj: obj, value: obj.Rate, event: e });
        ComplaintItem.sumOfTotalItem();
    }
}

window.tableItemUnit = (value, obj, index) => {
    return `${obj.UnitDesc ? obj.UnitDesc : '-'}`;
}
window.tableItemQty = (value, obj, index) => {
    return `
        <input type="text" id="Qty_${index}" class="form-control form-control-sm text-right item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}
window.tableItemQtyEvent = {
    'input .item-qty': (e, value, obj, index) => {        
        obj.Qty = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableItem', index: index, obj: obj, value: obj.Qty, event: e });
        ComplaintItem.sumOfTotalItem();
    }
}
window.tableItemAmount = (value, obj, index) => {
    return `
    <input type="text" id="Amount_${index}" class="form-control form-control-sm text-right item-amt" value="${obj.Amount}" oninput="this.value = _Number.validate({value: this.value, dp: 2, min: 0, max: 99999999})">
    `;
}
window.tableItemAmountEvent = {
    'input .item-amt': (e, value, obj, index) => {
        obj.Amount = e.currentTarget.value == '' ? '0' : e.currentTarget.value;        
        Table.updateByIndex({ id: '#tableItem', index: index, obj: obj, value: obj.Amount, event: e });
    }
}
window.tableItemAction = (value, obj, index) => {
    return `
        <button type="button" class="btn btn-sm btn-outline-danger rounded-5 item-delete" title="${obj.IsDelete ? 'Delete Item' : 'Remove Item'}">
            <span class="fa ${obj.IsDelete ? 'fa-trash' : 'fa-times'}"></span>
        </button>
    `
}
window.tableItemActionEvent = {
    'click .item-delete': (e, value, obj, index) => {
        ComplaintItem.delete({ obj: obj, index: index });
    }
}


