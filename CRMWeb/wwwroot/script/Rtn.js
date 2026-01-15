class Rtn
{
    static item = [];
    static init()
    {
        Rtn.getViewOption();
        $('#btnSearch').on('click', () => {
            Rtn.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableRtn', data: response.data });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Rtn.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "RTN" });
                }
            });
        });
        $('#btnNewEntry').on('click', () => {
            Rtn.newEntry();
        });
        let previousGdnIds = [];
        $('#Rtn-GdnNoId').on('change', () => {

            let currentGdnIds = $('#Rtn-GdnNoId').val() || [];
            currentGdnIds = currentGdnIds.map(x => parseInt(x));
            let addedGdn = currentGdnIds.filter(x => !previousGdnIds.includes(x));
            let removedGdn = previousGdnIds.filter(x => !currentGdnIds.includes(x));

            let tableData = $('#tableRtnItem').bootstrapTable('getData');
            if (removedGdn.length > 0) {
                tableData = tableData.filter(x => !removedGdn.includes(x.SourceVoucherId));
            }
            if (addedGdn.length === 0) {
                Table.add({
                    id: '#tableRtnItem',
                    data: tableData,
                    selectPick: true
                });
                previousGdnIds = currentGdnIds;
                return;
            }
            Message.confirm({
                msg: 'Do you want to load DLN items?',
                confirmButtonText: 'Yes',
                denyButtonText: 'No',
                data: addedGdn,
                onConfirm: (addedGdn) => {

                    Data.post({
                        url: 'Rtn/GetGdnItem',
                        data: addedGdn,
                        onSuccess: (response) => {

                            //let dlnItems = response.data.map(x => {
                            //    x.SourceVoucherId = x.SourceVoucherId || x.VoucherId;
                            //    return x;
                            //});
                            let dlnItems = response.data.map(x => {
                                return {
                                    ...x,Id: 0, VoucherId: 0,SourceVoucherId: x.SourceVoucherId || x.VoucherId
                                };
                            });

                            let uniqueMap = {};

                            [...tableData, ...dlnItems].forEach(x => {

                                let key = [ x.SourceVoucherId || 'MANUAL',x.ItemId || 0,x.SerialNo || '',x.BatchNo || '' ].join('_');

                                if (!uniqueMap[key]) {
                                    uniqueMap[key] = x;
                                }
                            });
                            let finalData = Object.values(uniqueMap);
                            Table.add({id: '#tableRtnItem', data: finalData,selectPick: true});
                            previousGdnIds = currentGdnIds;
                        }
                    });
                },onDeny: () => {Dropdown.setValue({id: '#Rtn-GdnNoId', value: previousGdnIds});}
            });
        });
        $('#Rtn-BtnSave').on('click', () => {

            if (!Field.isMandatory({ class: '.required' })) return;
            let isEdit = !!$('#Id').val();  
            let voucherItem = $('#tableRtnItem').bootstrapTable('getData')
                .filter(x => x.ItemId > 0 && x.Qty > 0)
                .map(x => {if (!isEdit) {x.Id = 0;x.VoucherId = 0;} return x;});

            if (voucherItem.length === 0) {
                Message.error({ statusText: 'Add at least one RTN item' });
                return;
            }

            let obj = Data.serializeToObject({ formId: '#formRtn' });
            obj.VoucherItem = voucherItem;
            obj.ListVoucherId = JSON.stringify($('#Rtn-GdnNoId').val() || []);

            if (!obj.Id) {
                Rtn.add(obj);      
            } else {
                Rtn.update(obj);    
            }
        });
    }
    static getViewOption() {
        Data.get({
            url: 'Rtn/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListNo', data: response.data.Number, value: 'Id', text: 'No' });
                Dropdown.bind({ id: '#ListPartyId', data: response.data.Party, value: 'Id', text: 'Name' });
            }
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'Rtn/GetAddOption', onSuccess: onSuccess });
    }
    static get({ method = 'Get', onSuccess }) {

        let obj = {
            ListStatus: $('#ListStatus').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListNo: $('#ListNo').val(),
            ListPartyId: $('#ListPartyId').val()
        };

        Data.post({ url: `Rtn/${method}`, data: obj, onSuccess: onSuccess });
    }
    static newEntry() {

        Rtn.getAddOption({
            onSuccess: (response) => {

                Rtn.item = response.data.Item || [];

                Dropdown.bind({ id: '#Rtn-Store', data: response.data.Store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Rtn-GdnNoId', data: response.data.GdnNo, value: 'Id', text: 'No' });

                Modal.open({ id: '#modalRtn', title: 'RTN / Add', action: 'Add' });
            }
        });
    }
    static addRtnItem() {

        let obj = {
            Id: 0,
            VoucherId: 0,
            ItemId: 0,
            ItemDesc: null,
            SerialNo: null,
            BatchNo: null,
            Qty: 0,
            Rate: 0,
            Amount: 0,
            Remarks: null
        };

        Table.add({
            id: '#tableRtnItem',
            data: obj,
            action: 'append',
            selectPick: true
        });
    }
    static add(obj) {
        Data.post({
            url: 'Rtn/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalRtn' });
                    Table.add({ id: '#tableRtn', data: response.obj, action: 'prepend' });
                }
            }
        });
    }
    static edit({ id, action = 'Edit' }) {

        Data.get({
            url: `Rtn/Edit?Id=${id}`,
            onSuccess: (response) => {

                let option = response.data;
                let obj = response.obj;

               
                Rtn.item = option.Item || [];

                let storeId = null;
                if (obj.VoucherItem && obj.VoucherItem.length > 0) {
                    storeId = obj.VoucherItem[0].StoreId;
                }


                let title = action === 'Edit'
                    ? `RTN / Edit (No: ${obj.No})`
                    : `RTN / Add`;

                Modal.open({id: '#modalRtn',title: title,action: action,obj: obj});
                Dropdown.bind({ id: '#Rtn-Store',data: option.Store || [], value: 'Id', text: 'Description',initialValue: storeId ? [storeId] : []});
                let gdnIds = [];
                if (obj.ListVoucherId) {
                    try {
                        gdnIds = JSON.parse(obj.ListVoucherId);
                    } catch (e) {
                        console.error("Invalid ListVoucherId:", obj.ListVoucherId);
                    }
                }
                Dropdown.bind({id: '#Rtn-GdnNoId',data: option.GdnNo || [],value: 'Id', text: 'No',initialValue: gdnIds });
                Table.add({id: '#tableRtnItem',data: obj.VoucherItem || [],selectPick: true });
             
            }
        });
    }
    static update(obj) {
        Data.update({
            url: 'Rtn/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalRtn' });
                    Table.updateById({ id: '#tableRtn', objId: response.obj.Id, obj: response.obj });
                }
            }
        });
    }
}
window.tableRtnSlNo = (value, obj, index) => index + 1;
window.tableRtnDate = (value, obj, index) => {
    return obj.Date ? moment(obj.Date).format('DD-MMM-YYYY') : '';
};
window.tableRtnStore = function (value, row, index) {
    if (!value) {
        return '<span class="text-muted">N/A</span>';
    }
    return `
        <div class="store-wrapper">
            <i class="fa-solid fa-store me-1"></i> 
            <strong>${value}</strong>
        </div>
    `;
};
window.tableRtnStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
};
window.tableRtnCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName ?? ''}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
};
window.tableRtnUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName ?? ''}</div>
        <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
};
window.tableRtnAction = (value, obj, index) => {
    let actionBtn = [];

    if (obj.IsEdit) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-success-100 btn-edit">
                    <span class="fa fa-edit"></span>&nbsp;&nbsp;View / Edit
                </a>
            </li>
        `);
    }

    if (obj.IsDelete) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-danger-100 btn-delete">
                    <span class="fa fa-trash"></span>&nbsp;&nbsp;Delete
                </a>
            </li>
        `);
    }

    return `
        <div class="btn-group dropstart">
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${actionBtn.join('')}
            </ul>
        </div>
    `;
};
window.tableRtnActionEvent = {
    'click .btn-edit': function (e, value, row, index) {
        e.preventDefault();
        Rtn.edit({ id: row.Id });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Rtn.delete({ id: obj.Id });
    }
};
window.tableRtnItemSlNo = (value, obj, index) => index + 1;
window.tableRtnItemDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({
        id: `RtnItem_${index}`,
        className: 'rtn-item',
        data: Rtn.item,
        value: 'Id',
        text: 'Description',
        initialValue: [obj.ItemId],
        json: true,
        parent: '.modal'
    })}
        <input type="text"
               id="RtnItemRemarks_${index}"
               class="form-control form-control-sm mb-0 mt-1 rtn-item-remarks"
               maxlength="100"
               placeholder="Remarks"
               value="${obj.Remarks ?? ""}">
    `;
};
window.tableRtnItemDescEvent = {

    // When item is selected
    'change .rtn-item': (e, value, obj, index) => {

        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value)
            ? parseInt(e.currentTarget.value)
            : 0;

        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });

        obj.ItemDesc = itemJson?.Description ?? null;
        obj.Rate = itemJson?.Rate ?? 0;

        obj.Amount = (parseFloat(obj.Rate || 0) * parseFloat(obj.Qty || 0)).toFixed(2);

        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.ItemId,
            event: e
        });
    },

    'input .rtn-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.Remarks,
            event: e
        });
    }
};
window.tableTaskSerialNo = (value, obj, index) => {
    return `
        <input type="text"
               id="RtnSerialNo_${index}"
               class="form-control form-control-sm rtn-item"
               value="${obj.SerialNo ?? ''}"
               maxlength="150" />
    `;
};
window.tableRtnSerialNoDescEvent = {
    'input .rtn-item': (e, value, obj, index) => {
        obj.SerialNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.SerialNo,
            event: e
        });
    }
};
window.tableTaskBatchNo = (value, obj, index) => {
    return `
        <input type="text"
               id="RtnBatchNo_${index}"
               class="form-control form-control-sm rtn-item"
               value="${obj.BatchNo ?? ''}"
               maxlength="150" />
    `;
};
window.tableRtnBatchNoDescEvent = {
    'input .rtn-item': (e, value, obj, index) => {
        obj.BatchNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.BatchNo,
            event: e
        });
    }
};
window.tableRtnItemQty = (value, obj, index) => {
    return `
        <input type="text"
               id="RtnItemQty_${index}"
               class="form-control form-control-sm text-right rtn-item-qty"
               value="${obj.Qty}"
               oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
};
window.tableRtnItemQtyEvent = {
    'input .rtn-item-qty': (e, value, obj, index) => {

        obj.Qty = e.currentTarget.value === '' ? '0' : e.currentTarget.value;
        obj.Amount = (parseFloat(obj.Rate || 0) * parseFloat(obj.Qty || 0)).toFixed(2);

        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.Qty,
            event: e
        });
    }
};
window.tableRtnItemAmount = (value, obj, index) => {
    return `
        <input type="text"
               id="RtnItemAmt_${index}"
               class="form-control form-control-sm text-right rtn-item-amount"
               value="${obj.Amount}"
               readonly>
    `;
};
Rtn.delete = ({ id }) => {
    Message.confirm({
        msg: "Do you want to delete this RTN?",
        confirmButtonText: "Delete",
        denyButtonText: "Cancel",
        data: id,
        onConfirm: (id) => {
            Data.delete({
                url: `Rtn/Delete?Id=${id}`,
                onSuccess: (response) => {
                    Message.show(response);
                    if (response.status === Message.Type.success) {
                        Table.updateById({
                            id: "#tableRtn",
                            objId: response.obj.Id,
                            obj: response.obj
                        });
                    }
                }
            });
        }
    });
};
