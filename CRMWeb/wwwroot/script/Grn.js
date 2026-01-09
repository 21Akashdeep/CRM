class Grn{ 
static item = [];
    static init() {

    Grn.getViewOption();

    $('#btnNewEntry').on('click', () => {
        Grn.newEntry();
    });

    $('#btnSearch').on('click', () => {
        Grn.get({
            method: 'Get',
            onSuccess: (response) => {
                Table.add({ id: '#tableGrn', data: response.data });
            }
        });
    }); 

    $('#btnPrint').on('click', () => {
        Grn.get({
            method: 'Print',
            onSuccess: (response) => {
                Table.add({ id: '#tableGrn', data: response.data, isPrint: true });
            }
        });
    });
    $('#btnExport').on('click', () => {
        Grn.get({
            method: 'Export',
            onSuccess: (response) => {
                Export.Base64ToExcel({ base64: response.base64, fileName: 'Grn' });
            }
        });
    });
    $('#Grn-BtnSave').on('click', () => {
        if (!Field.isMandatory({ class: '.required' })) {
            return;
        }

        let GrnItem = $('#tableGrnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);

        if (GrnItem.length == 0) {

            Message.error({ statusText: 'Grn Item not found. Add atleast one Grn Item' });
            return;
        }

        let obj = Data.serializeToObject({ formId: '#formGrn' });

        obj.GrnItem = GrnItem;

        obj.NetAmount = GrnItem.reduce((sum, x) => sum + (Number(x.Amount) || 0), 0).toFixed(2);

        if (!obj.Id) {
            Grn.add(obj);
        }
        else {
            Grn.update(obj);
        }

    });
}
    static getViewOption() {
    Data.get({
        url: 'Grn/GetViewOption',
        onSuccess: (response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListCustomerId', data: response.data.Customer, value: 'Id', text: 'Name' });
            Dropdown.bind({ id: '#ListConName', data: response.data.ConName, value: 'Id', text: 'ConName' });
        }
    });
}
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'Grn/GetAddOption', onSuccess: onSuccess });
    }

    static get({ method = 'Get', onSuccess }) {
    let obj = {
        ListStatus: $('#ListStatus').val(),
        //FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
        //ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
        ListCustomerId: $('#ListCustomerId').val(),
        ListConName: $('#ListConName').val()
    };
        Data.post({
            url: `Grn/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static newEntry() {
        Grn.getAddOption({
            onSuccess: (response) => {
                Grn.item = response.data.Item;
                Dropdown.bind({ id: '#Grn-PartyId', data: response.data.Party, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Grn-ShiftId', data: response.data.Shift, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalGrn', title: 'Grn / Add', action: 'Add' });
            }
        });
    }
    //static addGrnItem() {
    //    let obj = {
    //        GrnId: 0,
    //        ItemId: 0,
    //        ItemDesc: null,
    //        Qty: 0,
    //        Rate: 0,
    //        UnitDesc: null,
    //        Amount: 0,
    //        Remarks: null
    //    };
    //    Table.add({ id: '#tableGrnItem', data: obj, action: 'append', selectPick: true });
    //    Grn.sumOfTotalGrnItem();
    //}
    //static sumOfTotalGrnItem() {
    //    let GrnItem = $('#tableGrnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
    //    let Qty = GrnItem.length == 0 ? 0 : GrnItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);
    //    let Amount = GrnItem.length == 0 ? 0 : GrnItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
    //    let tfoot = `
    //        <tfoot>
    //            <tr>
    //                <th class="text-right" colspan="4">Total</th>
    //                <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>
    //                <th class="text-right">${_Number.format({ num: Amount, dp: 3, currency: 'INR' })}</th>
    //            </tr>
    //        </tfoot>
    //    `;
    //    $('#tableGrnItem tfoot').remove();
    //    $('#tableGrnItem').append(tfoot);
    //}
    //static add(obj) {
    //    Data.post({
    //        url: 'Grn/Add',
    //        data: obj,
    //        onSuccess: (response) => {
    //            Message.show(response);
    //            if (response.status == Message.Type.success) {
    //                Modal.reset({ id: '#modalGrn' });
    //                Table.add({ id: '#tableGrn', data: response.obj, action: 'prepend' });
    //                Dropdown.bind({ id: '#ListId', data: response.data.ViewOption.Grn, value: 'Id', text: 'GrnNo', subText: 'Date' });
    //            }
    //        }
    //    });
    //}
    //static edit({ id, action = 'Edit' }) {
    //    Data.get({
    //        url: Grn/Edit?Id=${id},
    //        onSuccess: (response) => {
    //            let option = response.data;
    //            Grn.item = option.Item;
    //            let obj = response.obj;
    //            obj.Id = action == 'Edit' ? obj.Id : null;
    //            let title = action == 'Edit' ? Grn / Edit (Code: ${obj.GrnNo}) : Grn / Add;
    //            Dropdown.bind({ id: '#Grn-PartyId', data: option.Party, value: 'Id', text: 'Description' });
    //            Dropdown.bind({ id: '#Grn-ShiftId', data: option.Shift, value: 'Id', text: 'Description' });
    //            Modal.open({ id: '#modalGrn', title: title, action: action, obj: obj });
    //            Table.add({ id: '#tableGrnItem', data: obj.GrnItem, selectPick: true });
    //            Grn.sumOfTotalGrnItem();
    //        }
    //    });
    //}
    //static update(obj) {
    //    Data.update({
    //        url: 'Grn/Update',
    //        data: obj,
    //        onSuccess: (response) => {
    //            Message.show(response);
    //            if (response.status == Message.Type.success) {
    //                Modal.close({ id: '#modalGrn' });
    //                Table.updateById({ id: '#tableGrn', objId: response.obj.Id, obj: response.obj });
    //            }
    //        }
    //    });
    //}
    //static delete({ id }) {
    //    Data.delete({
    //        url: Grn/Delete?Id=${id},
    //        onSuccess: (response) => {
    //            Message.show(response);
    //            if (response.status == Message.Type.success) {
    //                Table.updateById({ id: '#tableGrn', objId: response.obj.Id, obj: response.obj });
    //            }
    //        }
    //    });
    //}
}

window.tableGrnRefNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.RefNo ?? ''}</div>
        <div>${obj.RefDate ? moment(obj.RefDate).format('DD-MMM-YYYY') : ''}</div>
    `;
};
window.tableGrnSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableGrnDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableGrnStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableGrnCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
}
window.tableGrnUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${DateTime.dateTime(obj.UpdatedAt)}</div>
    `;
}
window.tableGrnAction = (value, obj, index) => {
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
window.tableGrnConAddress = (value, obj, index) => {

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
window.tableGrnActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Grn.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Grn.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        Grn.print({ id: obj.Id });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Grn.delete({ id: obj.Id });
    }
}

//Table Grn Item
//window.tableGrnItemSlNo = (value, obj, index) => {
//    return index + 1;
//}
//window.tableGrnItemDesc = (value, obj, index) => {
//    return `
//        ${Dropdown.html({ id: GrnItem_${index}, className: 'Grn-item', data: Grn.item, value: 'Id', text: 'Description', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
//        <input type="text" id="GrnItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 Grn-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
//    `;
//}
//window.tableGrnItemDescEvent = {
//    'change .Grn-item': (e, value, obj, index) => {
//        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
//        let itemJson = Dropdown.itemJson({ id: #${e.currentTarget.id} });
//        obj.UnitDesc = itemJson?.UnitDesc ?? null;

//        obj.Rate = itemJson?.Rate ?? 0;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

//        Table.updateByIndex({ id: '#tableGrnItem', index: index, obj: obj, value: obj.ItemId, event: e });
//        Grn.sumOfTotalGrnItem();
//    },
//    'input .Grn-item-remarks': (e, value, obj, index) => {
//        obj.Remarks = e.currentTarget.value;
//        Table.updateByIndex({ id: '#tableGrnItem', index: index, obj: obj, value: obj.Remarks, event: e });
//    }
//}
//window.tableGrnItemRate = (value, obj, index) => {
//    return `
//        <input type="text" id="GrnItemRate_${index}" class="form-control form-control-sm text-right Grn-item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
//    `;
//}
//window.tableGrnItemRateEvent = {
//    'input .Grn-item-rate': (e, value, obj, index) => {
//        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
//        Table.updateByIndex({ id: '#tableGrnItem', index: index, obj: obj, value: obj.Rate, event: e });
//        Grn.sumOfTotalGrnItem();
//    }
//}
//window.tableGrnItemQty = (value, obj, index) => {
//    return `
//        <input type="text" id="GrnItemQty_${index}" class="form-control form-control-sm text-right Grn-item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
//    `;
//}
//window.tableGrnItemQtyEvent = {
//    'input .Grn-item-qty': (e, value, obj, index) => {
//        obj.Qty = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
//        Table.updateByIndex({ id: '#tableGrnItem', index: index, obj: obj, value: obj.Qty, event: e });
//        Grn.sumOfTotalGrnItem();
//    }
//}
//window.tableGrnItemAmount = (value, obj, index) => {
//    return `
//        <input type="text" id="GrnItemAmt_${index}" class="form-control form-control-sm text-right Grn-item-amount" value="${obj.Amount}" oninput="this.value = _Number.validate({value: this.value, dp: 2, min: 0, max: 999999999999})">
//    `;
//}
//window.tableGrnItemAmountEvent = {
//    'input .Grn-item-amount': (e, value, obj, index) => {
//        obj.Amount = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        Table.updateByIndex({ id: '#tableGrnItem', index: index, obj: obj, value: obj.Amount, event: e });
//        Grn.sumOfTotalGrnItem();
//    }
//}