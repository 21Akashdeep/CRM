class Gdn {
    static item = [];
    static itemWithSerialNo = []; 
    static newitem = [];
    static newitemwithserialno = [];
    static unit = [];
    static conFator;
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
                    Table.add({ id: '#tableGdn', data: response.data, isPrint : true });
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
        Party.initAdd();
        Party.addOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalParty' });
            Gdn.getAddOption({
                onSuccess: (response) => {
                    Dropdown.bind({ id: '#Gdn-PartyId', data: response.data.Party, value: 'Id', text: 'Description', json: true, initialValue: [obj.Id] });
                    Field.triggerOnChange('#Gdn-PartyId');
                }
            });
        }
        Party.updateOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalParty' });
            setTimeout(() => {
                Gdn.getAddOption({
                    onSuccess: (response) => {
                        Dropdown.bind({ id: '#Gdn-PartyId', data: response.data.Party, value: 'Id', text: 'Description', json: true, initialValue: [obj.Id] });
                        Field.triggerOnChange('#Gdn-PartyId');
                    }
                });
            }, 500);
        }
        $('#Gdn-StockType').on('change', () => {

            let store = $('#Gdn-StoreId').val()|| 0;
            let stockType = $('#Gdn-StockType').val() || 0;         
            if ( store == 0 ||  stockType == 0) {
                Message.error({ statusText: 'Store is not selected!!!' });
                return;
            }
            let obj = {
                ListStoreId: [$('#Gdn-StoreId').val()],
                ListStockType: stockType
            };
            Data.post({
                url: 'Gdn/GetStockItem',
                data: obj,
                onSuccess: (response) => {
                    Gdn.item = response.data;
                }
            });
        });
        $('#Gdn-ConPinCode').on('input', () => {
            OnlineApi.pinCode({
                pinCode: $('#Gdn-ConPinCode').val(),
                callback: (data) => {
                    Dropdown.bind({ id: '#Gdn-ConPostOffice', data: data.obj.PostOfficeList, value: 'Name', text: 'Name', isEditable: true });
                    Dropdown.set({ id: '#Gdn-ConStateName', text: [data.obj.State] });
                }
            });
        });
        $('#GdnScan-ItemSerialNo').on('keydown', (e) => {
            if (e.key != 'Enter') return;
            let item = Gdn.itemWithSerialNo.find(x => x.SerialNo == $('#GdnScan-ItemSerialNo').val());
            if (!item) {
                Message.error({ statusText: `Invalid Serial No. (${$('#GdnScan-ItemSerialNo').val()})` });
                return;
            }                      
            Gdn.addGdnItem({
                itemId: item.ItemId,
                itemDesc: item.ItemDesc,
                expiryOn: item.ExpiryOn,
                serialNo: item.SerialNo,
                unitDesc: item.UnitDesc,
                qty: 1,
                baseQty:1,
                isScanned: true,
                callback: (obj) => {
                    Table.add({ id: '#tableGdnScanItem', data: obj, action: 'prepend' });
                    $('#GdnScan-ItemSerialNo').val('');
                }
            });            
        });

       

        $('#GdnScan-BtnAddItem').on('click', () => {
            let scanItem = $('#tableGdnScanItem').bootstrapTable('getData');
            if (scanItem.length === 0) {
                Message.error({ statusText: 'No scanned items found' });
                return;
            }            
            Table.add({ id: '#tableGdnItem', data: scanItem, action: 'append', selectPick: true });
            Gdn.sumOfTotalGdnItem();
            Modal.close({ id: '#modalGdnItemScan' });
        });
        $('#Gdn-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let gdnItem = $('#tableGdnItem').bootstrapTable('getData');
            if (gdnItem.length == 0) {
                Message.error({ statusText: 'Gdn Item not found.<br>Add atleast one Gdn Item.' });
                return;
            }
            let ZeroQtyItem = gdnItem.filter(x => x.Qty == 0);
            if (ZeroQtyItem.length != 0) {
                Message.error({ statusText: 'Qty Zero Input in Any Item!!!' });
                return;
            }
            let obj = Data.serializeToObject({ formId: '#formGdn' });                           
            obj.VoucherItem = gdnItem;
            obj.NetAmount = gdnItem.reduce((sum, x) => sum + (Number(x.Amount) || 0), 0).toFixed(2);
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
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListPartyId', data: response.data.Party, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#ListNo', data: response.data.Number, value: 'Id', text: 'No' });
            }
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'Gdn/GetAddOption', onSuccess: onSuccess });
    }
    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            Date: DateTime.json($('#Gdn-Date').val().split('|')[0]),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListPartyId: $('#ListPartyId').val(),            
            ListNo: $('#ListNo').val()
        };
        Data.post({
            url: `Gdn/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static newEntry() {
        Gdn.getAddOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#Gdn-StoreId', data: response.data.Store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Gdn-StockType', data: response.data.StockType, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#Gdn-PartyId', data: response.data.Party, value: 'Id', text: 'Name' });                
                Dropdown.bind({ id: '#Gdn-ConStateName', data: response.data.State, value: 'Name', text: 'Name' });
                Modal.open({ id: '#modalGdn', title: 'Gdn / Add', action: 'Add' });
                $('#Gdn-PoDate, #Gdn-EwayDate').val('');
            }
        });
    }
    static scanItem() {
        const storeId = $('#Gdn-StoreId').val();
        const stockType = $('#Gdn-StockType').val();
        if (Field.isNullOrEmpty(storeId) || Field.isNullOrEmpty(stockType)) {
            Message.error({ statusText: 'Store and Stock Type must be selected!!!' });
            return;
        }
        let obj = {
            ListStoreId: [storeId],
            ListStocktype: stockType
        };
        Data.post({
            url: 'Gdn/GetStockItemWithSerialNo',
            data: obj,
            onSuccess: (response) => {
                Modal.open({ id: '#modalGdnItemScan', title: 'Gdn / Scan Item' });             
                Gdn.itemWithSerialNo = response.data;
            }
        });
    }
    static getUnit(id, callback) {
        Data.get({
            url: `Gdn/getUnit?Id=${id}`,
            onSuccess: (response) => {
                callback(response.data); 
            }
        });
    }
    static addGdnItem({ itemId = 0, itemDesc = null, serialNo = null, expiryOn = null, qty = 0, baseQty = 0, unitDesc = null, isScanned = false, callback } = {}) {        
        if (Field.isNullOrEmpty($('#Gdn-StoreId').val()) && Field.isNullOrEmpty($('#Gdn-StockType').val()) ) {
            Message.error({ statusText: 'Please Select Both Store And StockType.' });
            return;
        }
        let obj = {
            Id: 0,
            ItemId: itemId,
            VoucherId: 0,
            StoreId: $('#Gdn-StoreId').val(),
            StoreDesc: $('#Gdn-StoreId option:selected').text(),
            ItemDesc: itemDesc,
            SerialNo: serialNo,
            BatchNo: null,
            ExpiryOn: expiryOn,
            Qty: qty,
            ConversionFactor :0,
            Rate: 0,
            BalQty: 0,
            BaseQty: baseQty,
            UnitDesc: unitDesc,
            Amount: 0,
            DiscountAmount: 0,
            TotalAmount: 0,
            ListTax: null,
            TaxRate: 0,
            TaxAmount: 0,
            GrossAmount: 0,
            ImageUrl: null,
            ReasonCode: null,
            Remarks: null,
            IsScanned: isScanned
        };
        console.log(obj);
        if (!callback) {
            Table.add({ id: '#tableGdnItem', data: obj, action: 'append', selectPick: true });
            Gdn.sumOfTotalGdnItem();
        }
        else {
            callback(obj);
        }
    }
    static addGdnScanItem({ ItemId, SerialNo, ExpiryOn, Qty }) {
        Table.add({ id: '#tableGdnScanItem', data: { ItemId, SerialNo, ExpiryOn, Qty }, action: 'append' });
    }
    static sumOfTotalGdnItem() {
        let GdnItem = $('#tableGdnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
        let Qty =  GdnItem.map(x => Math.abs(parseFloat(x.Qty) || 0)).reduce((s, v) => s + v, 0);
        let Amount = GdnItem.length == 0 ? 0 : GdnItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);

        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="5">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>
                    <th class="text-right"></th>
                </tr>
            </tfoot>
        `;
        $('#tableGdnItem tfoot').remove();
        $('#tableGdnItem').append(tfoot);
    }
    static add(obj) {
        Data.post({
            url: 'Gdn/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalGdn' });
                    Table.add({ id: '#tableGdn', data: response.obj, action: 'prepend' });
                }
            }
        });
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `Gdn/Edit?Id=${id}`,
            onSuccess: (response) => {    
                let option = response.data.AddOption;
                Dropdown.bind({ id: '#Gdn-StoreId', data: option.Store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Gdn-PartyId', data: option.Party, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Gdn-StockType', data: option.StockType, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#Gdn-ConStateName', data: option.State, value: 'Name', text: 'Name' });                
                Gdn.item = response.data.StockItem;
                Gdn.unit = response.data.Unit;
                let obj = response.data.Gdn;                
                obj.Id = action == 'Edit' ? obj.Id : null;
                let title = action == 'Edit' ? `Gdn / Edit (Code: ${obj.No})` : `Gdn / Add`;                
                Modal.open({ id: '#modalGdn', title: title, action: action, obj: obj });
                Table.add({ id: '#tableGdnItem', data: obj.GdnItem, selectPick: true });
                
                Gdn.sumOfTotalGdnItem();              
                let pincode = $('#Gdn-ConPinCode').val();
                if (pincode.length == 6) {
                    setTimeout(() => {
                        OnlineApi.pinCode({
                            pinCode: obj.ConPincode,
                            callback: (data) => {
                                Dropdown.bind({ id: '#Gdn-ConPostOffice', data: data.obj.PostOfficeList, value: 'Name', text: 'Name', isEditable: true, initialValue: [obj.ConPostOffice] });
                            }
                        });
                    }, 100);
                }                
            }
       });
    }
    static update(obj) {
        Data.update({
            url: 'Gdn/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalGdn' });
                    Table.updateById({ id: '#tableGdn', objId: response.obj.Id, obj: response.obj });
                }
            }
        });
    }
    static delete({ id }) {
        Message.confirm({
            msg: "Do you want to delete???",
            confirmButtonText: "Delete",
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({
                    url: `Gdn/Delete?Id=${id}`,
                    onSuccess: Gdn.deleteOnSuccess
                });
            }
        });
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableGdn", objId: response.obj.Id, obj: response.obj });
        }
    }
    static deleteScanItem({ id, index }) {
        Table.remove({ id: '#tableGdnScanItem', value: [index] });
    }
    static deleteItem({ id, index }) {
        Table.remove({ id: '#tableGdnItem', value: [index] });
    }
    static print({ obj }) {
        Data.post({
            url: "Gdn/Print",
            data: obj,
            onSuccess: (response) => {
                if (response.status !== Message.Type.success) {
                    Message.error(response);
                    return;
                }

                let reportTitle = 'Goods Delivery Notes';
                if (obj.FromDate && obj.TillDate) {
                    reportTitle += ` ${moment(obj.FromDate).format('DD-MMM-YYYY')} To ${moment(obj.TillDate).format('DD-MMM-YYYY')}`;
                }

                let printContent = [];
                let GdnList = Array.isArray(response.data) ? response.data : [];

                GdnList.forEach((obj, index) => {
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
                        <th>GDN No.</th>
                        <th>Date</th>
                        <th> From Store</th>
                        <th>Party</th>
                        <th>Po No</th>
                        <th>Remarks</th>
                        <th>Created By</th>
                    </tr>
                </thead>
                `);

                    table.push('<tbody>');

                    // ===== Gdn Master Row =====
                    table.push(`
                <tr>
                    <td class="text-center">${index + 1}</td>
                    <td>${obj.No}</td>
                    <td>${moment(obj.Date).format('DD-MMM-YYYY')}</td>
                    <td>${obj.StoreDesc || '-'}</td>
                    <td>${obj.PartyDesc || '-'}</td>
                    <td>${obj.PoNo || '-'}</td>                
                    <td>${obj.Remarks || '-'}</td>
                    <td>${obj.CreatedByName}</td>
                </tr>
                `);

                    // ===== Gdn Items =====
                    let items = obj.GdnItem || [];

                    table.push(`
                <tr>
                    <td colspan="10" style="border-bottom:none;">
                        <table>
                            <thead>
                                <tr>
                                    <th colspan="7">GDN Items</th>
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
                                ${items.map((row, idx) => ` note
                                    <tr>
                                        <td class="text-center">${idx + 1}</td>
                                        <td>${row.ItemDesc}</td>                                     
                                        <td>${row.ExpiryOn ? moment(row.ExpiryOn).format('DD-MMM-YYYY') : '-'}</td> 
                                         <td>${row.SerialNo ? row.SerialNo:"-"}</td>
                                        <td class="text-right">${Math.abs(row.Qty)}</td>
                                    </tr>
                                `).join('')}
                                <tr>
                                    <th colspan="4" class="text-right">Total</th>
                                    <th class="text-right">
                                       ${items.reduce((s, x) => {
                                           let qty = parseFloat(x.Qty);
                                           return s + (isNaN(qty) ? 0 : Math.abs(qty));
                                       }, 0)}
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

//GDN Table
window.tableGdnSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableGdnDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableGdnPoNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.PoNo ?? ''}</div>
        <div>${obj.PoDate ? moment(obj.PoDate).format('DD-MMM-YYYY') : ''}</div>
    `;
};
window.tableGdnStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
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
    let action = [];
    if (obj.IsEdit) {
        action.push(`
             <li>
                <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                    <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                </a>
            </li>
        `);
    }
    if (obj.IsDuplicate) {
        action.push(`
            <li>
                <a href="#" class="dropdown-item text-primary-100 btn-duplicate" title="Duplicate">
                    <span class="fa fa-copy text-primary-100"></span>&nbsp;&nbsp;Duplicate
                </a>
            </li>
        `);
    }
    if (obj.IsDelete) {
        action.push(`
            <li>
                <a href="#" class="dropdown-item text-danger-100 btn-delete" title="Delete">
                    <span class="fa fa-trash text-danger-100"></span>&nbsp;&nbsp;Delete
                </a>
            </li>
        `);
    }
    if (obj.IsEnable) {
        action.push(`
            <li>
                <a href="#" class="dropdown-item text-success-100 btn-enable" title="Enable">
                    <span class="fa fa-toggle-on text-success-100"></span>&nbsp;&nbsp;Enable
                </a>
            </li>
        `);
    }
    if (obj.IsPrint) {
        action.push(`
            <li>
                <a href="#" class="dropdown-item text-primary-100 btn-print" title="Print">
                    <span class="fa fa-print text-primary-100"></span>&nbsp;&nbsp;Print
                </a>
            </li>
        `);
    }
    return `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                   ${action.join('')}
            </ul>
        </div>
    `;    
}
window.tableGdnActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Gdn.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Gdn.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        Gdn.print({ obj: { ListId: [obj.Id] } });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Gdn.delete({ id: obj.Id });
    }
}
//Table GdnItem
window.tableGdnItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableGdnItemDesc = (value, obj, index) => {
    setTimeout(() => {
        if (!obj.UnitList && obj.ItemId) {
            Gdn.getUnit(obj.ItemId, (unitList) => {
                obj.UnitList = unitList;

                let selected = unitList[0];
                obj.UnitId = selected?.UnitId || 0;
                obj.UnitDesc = selected?.UnitDesc || null;

                Table.updateByIndex({
                    id: '#tableGdnItem',
                    index: index,
                    obj: obj
                });
            });
        }
    }, 0);

    return `
        ${Dropdown.html({ id: `GdnItem_${index}`, className: 'gdn-item', data: Gdn.item, value: 'ItemId', text: 'ItemDesc', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
        <input type="text" id="GdnItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 gdn-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
    `;
}
window.tableGdnItemDescEvent = {
    'change .gdn-item': (e, value, obj, index) => {       
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;  
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson?.UnitDesc ?? null;
        obj.UnitId = itemJson?.UnitId ?? 0;
        obj.Rate = itemJson?.Rate ?? 0;
        Gdn.getUnit(obj.ItemId, (unitList) => {

            obj.UnitList = unitList;
            obj.UnitId = unitList?.[0]?.Id || 0;
            obj.UnitDesc = unitList?.[0]?.UnitDesc || null;

            Table.updateByIndex({
                id: '#tableGdnItem',
                index: index,
                obj: obj,
                event: e
            });

        });
        let item = Gdn.item.find(x => x.ItemId == obj.ItemId);      
        /*item.SerialNo ? obj.SerialNo = item.SerialNo: obj.SerialNo = "N/A";*/
        obj.BalQty = item ? item.Qty : 0;  
        //Sum of Amount
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        //Update Row Data
        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.SerialNo, event: e });
        //Sum Total Amount
        Gdn.sumOfTotalGdnItem();
    },
    'input .gdn-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.Remarks, event: e });
    }
}
window.tableGdnItemUnitDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({
        id: `gdnUnit_${index}`,
        className: 'gdn-unit',
        data: obj.UnitList || [],   
        value: 'UnitId',
        text: 'UnitDesc',
        initialValue: [obj.UnitId],
        json: true,
        parent: '.modal'
    })}
    `;
}
window.tableGdnItemUnitDescEvent = {
    'change .gdn-unit': (e, value, obj, index) => {
        obj.UnitId = parseInt(e.currentTarget.value) || 0;
        let unitJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = unitJson?.UnitDesc || null;
        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            event: e
        });

        Gdn.sumOfTotalGdnItem();
    }
}

window.tableGdnItemExpiryOn = (value, obj, index) => {
    return obj.ExpiryOn ? moment(obj.ExpiryOn).format("DD-MMM-YYYY") : "-";
}
window.tableGdnItemQty = (value, obj, index) => {
    return `
        <div class="input-group">            
            <input type="text" id="GdnItemQty_${index}" class="form-control form-control-sm text-end gdn-item-qty" value="${Math.abs(obj.Qty)}" 
            oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})"/>
            <span class="input-group-text fw-bold">${obj.UnitDesc ?? 'NA'}</span>
        </div>
    `;
}
window.tableGdnItemQtyEvent = {
    'input .gdn-item-qty': (e, value, obj, index) => {

        let qty = parseFloat(e.currentTarget.value || 0);
        let balQty = parseFloat(obj.BalQty || 0);

        // minus qty check
        if (qty < 0) {
            Message.error({ statusText: 'Qty cannot be negative' });
            e.currentTarget.value = obj.Qty || 0;
            return;
        }

        // available qty check
        if (qty > balQty) {
            Message.error({ statusText: `Qty cannot be greater than available qty (${balQty})` });
            e.currentTarget.value = obj.Qty || 0;
            return;
        }

        obj.Qty = qty;
        let unitId = $(`#gdnUnit_${index}`).val();
        let selectedUnit = obj.UnitList?.find(x => x.UnitId == unitId);
        let cf = selectedUnit?.ConversionFactor || 1;
        obj.BaseQty = qty * cf;
        obj.ConversionFactor = cf;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            value: obj.Qty,
            event: e
        });

        Gdn.sumOfTotalGdnItem();
    }
};
window.tableGdnItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableGdnItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        Gdn.deleteItem({ id: obj.Id, index: index });
    }
}


//Table Scan
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
window.tableGdnScanItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableGdnScanItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        Gdn.deleteScanItem({ id: obj.Id, index: index });
    }
}
window.tableGdnScanExpiryOn = (value, obj, index) => {
    let val = obj.ExpiryOn
        ? moment(obj.ExpiryOn, ['DD-MMM-YYYY', 'YYYY-MM-DD']).format('YYYY-MM-DD')
        : '';

    return `
        <input type="date"
            class="form-control form-control-sm mb-0 Gdn-item-expiry"
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
