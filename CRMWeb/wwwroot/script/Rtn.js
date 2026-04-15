class Rtn
{
    static item = [];
    static Id;
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
        $('#RtnScan-ItemSerialNo').on('keydown', (e) => {
            if (e.key == 'Enter') {
                if (!Field.isMandatory({ class: '.scan-required' })) {
                    return;
                }
                let serialNo = $('#RtnScan-ItemSerialNo').val();
                let countRtnItem = $('#tableRtnItem').bootstrapTable('getData').filter(x => x.SerialNo == serialNo).length;
                let countScanItem = $('#tableRtnScanItem').bootstrapTable('getData').filter(x => x.SerialNo == serialNo).length;
                if (countRtnItem > 0 || countScanItem > 0) {
                    Message.error({ statusText: `Serial No. ${serialNo} already added in Scan List or Item List.` });
                    return;
                }
                Rtn.addRtnItem({
                    itemId: $('#RtnScan-ItemId').val(),
                    itemDesc: $('#RtnScan-ItemId option:selected').text(),
                    expiryOn: $('#RtnScan-ItemExpiryOn').val(),
                    serialNo: serialNo,
                    qty: 1,
                    isScanned: true,
                    callback: (obj) => {
                        Table.add({ id: '#tableRtnScanItem', data: obj, action: 'prepend' });
                        $('#RtnScan-ItemExpiryOn,#RtnScan-ItemSerialNo').val('');

                    }
                });
            }
        });

        $('#btnScanItemAdd').on('click', () => {
            let scanItems = $('#tableRtnScanItem').bootstrapTable('getData');
            if (scanItems.length === 0) {
                Message.error({ statusText: 'No scanned items found' });
                return;
            }
            Table.add({ id: '#tableRtnItem', data: scanItems, action: 'append', selectPick: true });
            Rtn.sumOfTotalRtnItem();

            Modal.close({ id: '#modalRtnItemScan' });

        });
        $('#Rtn-BtnSave').on('click', () => {

            if (!Field.isMandatory({ class: '.required' })) return;
            let isEdit = !!$('#Id').val();  


            let voucherItem = $('#tableRtnItem').bootstrapTable('getData');

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
                Rtn.Id = response.data.Id;

                Dropdown.bind({ id: '#Rtn-Store', data: response.data.Store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Rtn-GdnNoId', data: response.data.GdnNo, value: 'Id', text: 'No' });

                Modal.open({ id: '#modalRtn', title: 'RTN / Add', action: 'Add' });
            }
        });
    }
    static scanner() {
        Modal.open({ id: '#modalRtnItemScan', title: 'Rtn / Scan Item' });
        Dropdown.bind({ id: '#RtnScan-ItemId', data: Rtn.item, value: 'Id', text: 'Description' });
        $('#Rtn-ExpiryOn,#RtnScan-ItemSerialNo').val('');

    }
    static addRtnItem({ itemId = 0, itemDesc = null, serialNo = "", expiryOn = null, qty = 0, isScanned = false, callback } = {}) {
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
            ConversionFactor: 0,
            BaseQty : qty,
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
            Table.add({ id: '#tableRtnItem', data: obj, action: 'append', selectPick: true });
            Rtn.sumOfTotalRtnItem();
        }
        else {
            callback(obj);
        }
    }
    static addRtnScanItem({ ItemId, SerialNo, ExpiryOn, Qty }) {
        Table.add({ id: '#tableRtnScanItem', data: { ItemId, SerialNo, ExpiryOn, Qty }, action: 'append' });
    }
    static sumOfTotalRtnItem() {
        let RtnItem = $('#tableRtnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
        let Qty = RtnItem.map(x => Math.abs(parseFloat(x.Qty) || 0)).reduce((s, v) => s + v, 0);
        let Amount = RtnItem.length == 0 ? 0 : RtnItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="5">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>                    
                </tr>
            </tfoot>
        `;
        $('#tableRtnItem tfoot').remove();
        $('#tableRtnItem').append(tfoot);
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
                Rtn.Id = response.obj.Id;

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
    static deleteItems({ id,index }) {

        let obj = {             
            Id:Rtn.Id,
            DeleteItemId: id     
        };
        if (index >= 0 && id == 0 ) {
            Table.remove({ id: '#tableRtnItem', value: [index] });
            return;
        }
        Message.confirm({
            msg: "Do you want to delete?",
            confirmButtonText: "Delete",
            denyButtonText: "Don't Delete",
            onConfirm: () => {
                Data.post({
                    url: "Rtn/DeleteItem",
                    data: obj,
                    onSuccess: Rtn.deleteItemOnSuccess
                });
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
                            url: `Rtn/Delete?Id=${id}`,
                            onSuccess: Rtn.deleteOnSuccess
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
    static deleteItemOnSuccess = (response) => {
            Message.show(response);
        if (response.status == Message.Type.success) {
            let manualData = $('#tableRtnItem').bootstrapTable('getData').filter(x => x.Id == 0);
            let finaldata = manualData.concat(response.data[0].VoucherItem);
            Table.updateById({ id: "#tableTask", obj: response.data.obj });
            Table.add({ id: '#tableRtnItem', data: finaldata, selectPick: true })
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }
    static deleteItem({ id, index }) {
        Table.remove({ id: '#tableRtnScanItem', value: [index] });
    }
    static print({ obj }) {
        Data.post({
            url: "Rtn/Print",
            data: obj,
            onSuccess: (response) => {
                if (response.status !== Message.Type.success) {
                    Message.error(response);
                    return;
                }

                let reportTitle = 'Return Notes';
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
                    <th>Rtn No.</th>
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
                                <th colspan="7">Return Items</th>
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
                                     <td>${row.SerialNo ? row.SerialNo:'-'}</td>
                                    <td class="text-right">${row.Qty}</td>
                                </tr>
                            `).join('')}
                            <tr>
                                <th colspan="4" class="text-right">Total</th>
                                <th class="text-right">
                                   ${
                               (() => {
                            const uniqueUnits = Data.unique({ data: items, field: 'UnitId' });
                            return uniqueUnits.length === 1
                                ? items.reduce((s, x) => s + Number(x.Qty || 0), 0) : '';
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

window.tableRtnSlNo = (value, obj, index) => index + 1;
window.tableRtnDate = (value, obj, index) => {
    return obj.Date ? moment(obj.Date).format('DD-MMM-YYYY') : '';
};
window.tableRtnStore = function (value, row, index) {
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
//window.tableRtnAction = (value, obj, index) => {
//    let actionBtn = [];

//    if (obj.IsEdit) {
//        actionBtn.push(`
//            <li>
//                <a href="#" class="dropdown-item text-success-100 btn-edit">
//                    <span class="fa fa-edit"></span>&nbsp;&nbsp;View / Edit
//                </a>
//            </li>
//        `);
//    }

//    if (obj.IsDelete) {
//        actionBtn.push(`
//            <li>
//                <a href="#" class="dropdown-item text-danger-100 btn-delete">
//                    <span class="fa fa-trash"></span>&nbsp;&nbsp;Delete
//                </a>
//            </li>
//        `);
//    }

//    return `
//        <div class="btn-group dropstart">
//            <button class="btn btn-sm border-0" data-bs-toggle="dropdown">
//                <i class="fa fa-ellipsis-v"></i>
//            </button>
//            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
//                ${actionBtn.join('')}
//            </ul>
//        </div>
//    `;
//};
//window.tableRtnActionEvent = {
//    'click .btn-edit': function (e, value, row, index) {
//        e.preventDefault();
//        Rtn.edit({ id: row.Id });
//    },
//    'click .btn-delete': (e, value, obj, index) => {
//        Rtn.delete({ id: obj.Id });
//    }
//};

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
    'click .btn-edit': (e, value, obj, index) => {
        e.preventDefault();
        Rtn.edit({ id: obj.Id, action: 'Edit' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Rtn.delete({ id: obj.Id });
    },
    'click .btn-print': (e, value, obj, index) => {
        Rtn.print({ obj: { ListId: [obj.Id] } });
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
               value="${Math.abs(obj.Qty)}"
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
//Rtn.delete = ({ id }) => {
//    Message.confirm({
//        msg: "Do you want to delete this RTN?",
//        confirmButtonText: "Delete",
//        denyButtonText: "Cancel",
//        data: id,
//        onConfirm: (id) => {
//            Data.delete({
//                url: `Rtn/Delete?Id=${id}`,
//                onSuccess: (response) => {
//                    Message.show(response);
//                    if (response.status === Message.Type.success) {
//                        Table.updateById({
//                            id: "#tableRtn",
//                            objId: response.obj.Id,
//                            obj: response.obj
//                        });
//                    }
//                }
//            });
//        }
//    });
//};
window.tableRtnExpiryOn = (value, obj) => {
    return `${!obj.ExpiryOn ? '-' : moment(obj.ExpiryOn).format('DD-MM-YYYY')}`;
};

window.tableRtnItemDeleteAction = (value, obj, index) => {
    if (obj.Id || obj.Id ==0)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableRtnItemDeleteActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        Rtn.deleteItems({ id: obj.Id, index: index });
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
window.tableRtnScanItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableRtnScanItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        Rtn.deleteItem({ id: obj.Id, index: index });
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
