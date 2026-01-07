class Gdn {
    static item = [];
    static init() {
        Gdn.getViewOption();
        $('#btnNewEntry').on('click', () => {
            Gdn.newEntry();
        });
        $('#btnSearch').on('click', () => {
            Gdn.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableGdn', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Gdn.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableGdn', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Gdn.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: 'Gdn' });
                }
            });
        });
        $('#Gdn-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }

            let GdnItem = $('#tableGdnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);

            if (GdnItem.length == 0) {

                Message.error({ statusText: 'Gdn Item not found. Add atleast one Gdn Item' });
                return;
            }

            let obj = Data.serializeToObject({ formId: '#formGdn' });

            obj.GdnItem = GdnItem;

            obj.NetAmount = GdnItem.reduce((sum, x) => sum + (Number(x.Amount) || 0), 0).toFixed(2);

            if (!obj.Id) {
                Gdn.add(obj);
            }
            else {
                Gdn.update(obj);
            }

        });
    }
    static getViewOption() {
        Data.get({
            url: 'Gdn/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text:'Description' });
                Dropdown.bind({ id: '#ListCustomerId', data: response.data.Customer, value: 'Id', text: 'Name' });
                Dropdown.bind({ id: '#ListConName', data: response.data.ConName, value: 'Id', text: 'ConName' });
            }
        });
    }
    //static getAddOption({ onSuccess }) {
    //    Data.get({ url: 'Gdn/GetAddOption', onSuccess: onSuccess });
    //}

    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            //FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            //ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListCustomerId: $('#ListCustomerId').val(),
            ListConName: $('#ListConName').val()
        };
        Data.post({ url: `Gdn/${method}`, data: obj, onSuccess: onSuccess });
    }
    //static newEntry() {
    //    Gdn.getAddOption({
    //        onSuccess: (response) => {
    //            Gdn.item = response.data.Item;
    //            Dropdown.bind({ id: '#Gdn-PartyId', data: response.data.Party, value: 'Id', text: 'Description' });
    //            Dropdown.bind({ id: '#Gdn-ShiftId', data: response.data.Shift, value: 'Id', text: 'Description' });
    //            Modal.open({ id: '#modalGdn', title: 'Gdn / Add', action: 'Add' });
    //        }
    //    });
    //}
    //static addGdnItem() {
    //    let obj = {
    //        GdnId: 0,
    //        ItemId: 0,
    //        ItemDesc: null,
    //        Qty: 0,
    //        Rate: 0,
    //        UnitDesc: null,
    //        Amount: 0,
    //        Remarks: null
    //    };
    //    Table.add({ id: '#tableGdnItem', data: obj, action: 'append', selectPick: true });
    //    Gdn.sumOfTotalGdnItem();
    //}
    //static sumOfTotalGdnItem() {
    //    let GdnItem = $('#tableGdnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
    //    let Qty = GdnItem.length == 0 ? 0 : GdnItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);
    //    let Amount = GdnItem.length == 0 ? 0 : GdnItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
    //    let tfoot = `
    //        <tfoot>
    //            <tr>
    //                <th class="text-right" colspan="4">Total</th>
    //                <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>
    //                <th class="text-right">${_Number.format({ num: Amount, dp: 3, currency: 'INR' })}</th>                    
    //            </tr>
    //        </tfoot>
    //    `;
    //    $('#tableGdnItem tfoot').remove();
    //    $('#tableGdnItem').append(tfoot);
    //}
    //static add(obj) {
    //    Data.post({
    //        url: 'Gdn/Add',
    //        data: obj,
    //        onSuccess: (response) => {
    //            Message.show(response);
    //            if (response.status == Message.Type.success) {
    //                Modal.reset({ id: '#modalGdn' });
    //                Table.add({ id: '#tableGdn', data: response.obj, action: 'prepend' });
    //                Dropdown.bind({ id: '#ListId', data: response.data.ViewOption.Gdn, value: 'Id', text: 'GdnNo', subText: 'Date' });
    //            }
    //        }
    //    });
    //}
    //static edit({ id, action = 'Edit' }) {
    //    Data.get({
    //        url: `Gdn/Edit?Id=${id}`,
    //        onSuccess: (response) => {
    //            let option = response.data;
    //            Gdn.item = option.Item;
    //            let obj = response.obj;
    //            obj.Id = action == 'Edit' ? obj.Id : null;
    //            let title = action == 'Edit' ? `Gdn / Edit (Code: ${obj.GdnNo})` : `Gdn / Add`;
    //            Dropdown.bind({ id: '#Gdn-PartyId', data: option.Party, value: 'Id', text: 'Description' });
    //            Dropdown.bind({ id: '#Gdn-ShiftId', data: option.Shift, value: 'Id', text: 'Description' });
    //            Modal.open({ id: '#modalGdn', title: title, action: action, obj: obj });
    //            Table.add({ id: '#tableGdnItem', data: obj.GdnItem, selectPick: true });
    //            Gdn.sumOfTotalGdnItem();
    //        }
    //    });
    //}
    //static update(obj) {
    //    Data.update({
    //        url: 'Gdn/Update',
    //        data: obj,
    //        onSuccess: (response) => {
    //            Message.show(response);
    //            if (response.status == Message.Type.success) {
    //                Modal.close({ id: '#modalGdn' });
    //                Table.updateById({ id: '#tableGdn', objId: response.obj.Id, obj: response.obj });
    //            }
    //        }
    //    });
    //}
    //static delete({ id }) {
    //    Data.delete({
    //        url: `Gdn/Delete?Id=${id}`,
    //        onSuccess: (response) => {
    //            Message.show(response);
    //            if (response.status == Message.Type.success) {
    //                Table.updateById({ id: '#tableGdn', objId: response.obj.Id, obj: response.obj });
    //            }
    //        }
    //    });
    //}
}

window.tableGdnRefNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.RefNo ?? ''}</div>
        <div>${obj.RefDate ? moment(obj.RefDate).format('DD-MMM-YYYY') : ''}</div>
    `;
};
window.tableGdnSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableGdnDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableGdnStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}<div>`;
}
window.tableGdnCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
}
window.tableGdnUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${DateTime.dateTime(obj.UpdatedAt)}</div>
    `;
}
window.tableGdnAction = (value, obj, index) => {
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
window.tableGdnConAddress = (value, obj, index) => {

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
window.tableGdnActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Gdn.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Gdn.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        Gdn.print({ id: obj.Id });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Gdn.delete({ id: obj.Id });
    }
}

//Table Gdn Item
//window.tableGdnItemSlNo = (value, obj, index) => {
//    return index + 1;
//}
//window.tableGdnItemDesc = (value, obj, index) => {
//    return `
//        ${Dropdown.html({ id: `GdnItem_${index}`, className: 'Gdn-item', data: Gdn.item, value: 'Id', text: 'Description', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
//        <input type="text" id="GdnItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 Gdn-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
//    `;
//}
//window.tableGdnItemDescEvent = {
//    'change .Gdn-item': (e, value, obj, index) => {
//        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
//        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
//        obj.UnitDesc = itemJson?.UnitDesc ?? null;

//        obj.Rate = itemJson?.Rate ?? 0;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

//        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.ItemId, event: e });
//        Gdn.sumOfTotalGdnItem();
//    },
//    'input .Gdn-item-remarks': (e, value, obj, index) => {
//        obj.Remarks = e.currentTarget.value;
//        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.Remarks, event: e });
//    }
//}
//window.tableGdnItemRate = (value, obj, index) => {
//    return `
//        <input type="text" id="GdnItemRate_${index}" class="form-control form-control-sm text-right Gdn-item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
//    `;
//}
//window.tableGdnItemRateEvent = {
//    'input .Gdn-item-rate': (e, value, obj, index) => {
//        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
//        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.Rate, event: e });
//        Gdn.sumOfTotalGdnItem();
//    }
//}
//window.tableGdnItemQty = (value, obj, index) => {
//    return `
//        <input type="text" id="GdnItemQty_${index}" class="form-control form-control-sm text-right Gdn-item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
//    `;
//}
//window.tableGdnItemQtyEvent = {
//    'input .Gdn-item-qty': (e, value, obj, index) => {
//        obj.Qty = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
//        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.Qty, event: e });
//        Gdn.sumOfTotalGdnItem();
//    }
//}
//window.tableGdnItemAmount = (value, obj, index) => {
//    return `
//        <input type="text" id="GdnItemAmt_${index}" class="form-control form-control-sm text-right Gdn-item-amount" value="${obj.Amount}" oninput="this.value = _Number.validate({value: this.value, dp: 2, min: 0, max: 999999999999})">
//    `;
//}
//window.tableGdnItemAmountEvent = {
//    'input .Gdn-item-amount': (e, value, obj, index) => {
//        obj.Amount = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.Amount, event: e });
//        Gdn.sumOfTotalGdnItem();
//    }
//}