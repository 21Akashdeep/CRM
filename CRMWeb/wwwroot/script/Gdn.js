class Gdn {
    static item = [];
    static reasonCode = [];
    static store = [];
    static stateList = [];

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
            if (e.key == 'Enter') {
                if (!Field.isMandatory({ class: '.scan-required' })) {
                    return;
                }
                let serialNo = $('#GdnScan-ItemSerialNo').val();
                let countGdnItem = $('#tableGdnItem').bootstrapTable('getData').filter(x => x.SerialNo == serialNo).length;
                let countScanItem = $('#tableGdnScanItem').bootstrapTable('getData').filter(x => x.SerialNo == serialNo).length;
                if (countGdnItem > 0 || countScanItem > 0) {
                    Message.error({ statusText: `Serial No. ${serialNo} already added in Scan List or Item List.` });
                    return;
                }
                Gdn.addGdnItem({
                    itemId: $('#GdnScan-ItemId').val(),
                    itemDesc: $('#GdnScan-ItemId option:selected').text(),
                    expiryOn: $('#GdnScan-ItemExpiryOn').val(),
                    serialNo: serialNo,
                    qty: 1,
                    isScanned: true,
                    callback: (obj) => {
                        Table.add({ id: '#tableGdnScanItem', data: obj, action: 'prepend' });
                        $('#GdnScan-ItemSerialNo').val('');
                    }
                });
            }
        });

        $('#btnScanItemAdd').on('click', () => {
            let scanItems = $('#tableGdnScanItem').bootstrapTable('getData');
            if (scanItems.length === 0) {
                Message.error({ statusText: 'No scanned items found' });
                return;
            }
            Table.add({ id: '#tableGdnItem', data: scanItems, action: 'append', selectPick: true });
            Gdn.sumOfTotalGdnItem();
            Modal.close({ id: '#modalGdnItemScan' });
        });

        $('#Gdn-Store').on('change', () => {
            Gdn.getItemDetails();
        });

        $('#Gdn-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let GdnItem = $('#tableGdnItem').bootstrapTable('getData').filter(x => x.ItemId > 0)
                .map(x => {
                    if (x.ExpiryOn) {
                        x.ExpiryOn = moment(x.ExpiryOn).format('YYYY-MM-DD');
                    }
                    return x;
                });


            if (GdnItem.length == 0) {
                Message.error({ statusText: 'Gdn Item not found. Add atleast one Gdn Item' });
                return;
            }

            let obj = Data.serializeToObject({ formId: '#formGdn' });
            obj.VoucherItem = GdnItem;
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
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListPartyId', data: response.data.Party, value: 'Id', text: 'Name' });
              
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
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListPartyId: $('#ListPartyId').val(),
            //ListConName: $('#ListConName').val(),
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
                //Gdn.item = response.data.Item;
                Gdn.reasonCode = response.data.ReasonCode;
                let store = response.data.Store;
                let state = response.data.State;

                Dropdown.bind({ id: '#Gdn-ConStateCode', data: state, value: 'Code', text: 'Code', initialValue: [Gdn.stateList.Code] });
                Dropdown.bind({ id: '#Gdn-PartyId', data: response.data.Party, value: 'Id', text: 'Name' });
                Dropdown.bind({ id: '#Gdn-Store', data: store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Gdn-ConStateName', data: state, value: 'Name', text: 'Name', initialValue: [state.Name] });

                Modal.open({ id: '#modalGdn', title: 'Gdn / Add', action: 'Add' });
                $('#Gdn-RefDate, #Gdn-EwayDate').val('');
            }
        });
    }
    static scanner() {
        Modal.open({ id: '#modalGdnItemScan', title: 'Gdn / Scan Item' });
        Dropdown.bind({ id: '#GdnScan-ItemId', data: Gdn.item, value: 'Id', text: 'Description' });
        $('#Gdn-ExpiryOn').val('');
    }
    static getItemDetails() {

        let obj = {
            StoreId: $('#Gdn-Store').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            Todate: DateTime.json($('#DateRange').val().split('|')[1]) 

        };

        Data.post({
            url: 'Gdn/getItem',
            data: obj,
            onSuccess: (response) => {

                if (!response.data || response.data.length === 0)
                    return;

                Gdn.item = response.data.data;
                
            }
        });
    }

    static addGdnItem({ itemId = 0, itemDesc = null, serialNo = "", expiryOn = null, qty = 0, isScanned = false, callback } = {}) {

        


        let obj = {
            GdnId: 0,
            ItemId: itemId,
            VoucherId: 0,
            StoreId: 0,
            ItemDesc: itemDesc,
            SerialNo: serialNo,
            BatchNo: "",
            ExpiryOn: expiryOn,
            Qty: qty,
            Rate: 0,
            BalQty: 0, 
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
        let Qty = GdnItem.length == 0 ? 0 : GdnItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);
        let Amount = GdnItem.length == 0 ? 0 : GdnItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);

        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="6">Total</th>
                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>                    
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
                let option = response.data;
                //Gdn.item = option.Item;
                Gdn.reasonCode = response.data.ReasonCode;
                let store = response.data.Store;
                let state = response.data.State;
                let obj = response.obj;
                let StoreId = obj.VoucherItem[0].StoreId;
                obj.StoreId = StoreId;
                obj.Id = action == 'Edit' ? obj.Id : null;

                let title = action == 'Edit' ? `Gdn / Edit (Code: ${obj.No})` : `Gdn / Add`;

                Dropdown.bind({ id: '#Gdn-ConStateCode', data: state, value: 'Code', text: 'Code', initialValue: [state.Code] });
                Dropdown.bind({ id: '#Gdn-ConStateName', data: state, value: 'Name', text: 'Name', initialValue: [state.Name] });
                Dropdown.bind({ id: '#Gdn-PartyId', data: response.data.Party, value: 'Id', text: 'Name' });
                Dropdown.bind({ id: '#Gdn-Type', data: response.data.Type, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#Gdn-Store', data: store, value: 'Id', text: 'Description' });

                Modal.open({ id: '#modalGdn', title: title, action: action, obj: obj });
                Table.add({ id: '#tableGdnItem', data: obj.VoucherItem, selectPick: true });
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
    static deleteItem({ id, index }) {
        Table.remove({ id: '#tableGdnScanItem', value: [index] });
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

                let reportTitle = 'Delivery Notes';
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
                        <th>GDN No.</th>
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
        ${obj.IsPrint ?
            `
            <li>
                <a href="#" class="dropdown-item text-primary-100 btn-print" title="Print">
                    <span class="fa fa-print text-primary-100"></span>&nbsp;&nbsp;Print
                </a>
            </li>
        ` : ``
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
        Gdn.print({ obj: { ListId: [obj.Id] } });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Gdn.delete({ id: obj.Id });
    }
}


window.tableGdnItemSlNo = (value, obj, index) => {
    return index + 1;
}

window.tableGdnItemDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `GdnItem_${index}`, className: 'gdn-item', data: Gdn.item, value: 'ItemId', text: 'ItemDesc', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
        <input type="text" id="GdnItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 gdn-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
    `;
}

window.tableGdnItemDescEvent = {
    'change .gdn-item': (e, value, obj, index) => {
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        // find item from API result
        let item = Gdn.item.find(x => x.ItemId == obj.ItemId);

        // set balance qty
        obj.BalQty = item ? item.Qty : 0;

        obj.UnitDesc = itemJson?.UnitDesc ?? null;

        obj.Rate = itemJson?.Rate ?? 0;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.ItemId, event: e });
        Gdn.sumOfTotalGdnItem();
    },
    'input .gdn-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.Remarks, event: e });
    }
}

window.tableGdnItemRate = (value, obj, index) => {
    return `
        <input type="text" id="GdnItemRate_${index}" class="form-control form-control-sm text-right gdn-item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}

window.tableGdnItemRateEvent = {
    'input .gdn-item-rate': (e, value, obj, index) => {
        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.Rate, event: e });
        Gdn.sumOfTotalGdnItem();
    }
}

window.tableGdnItemQty = (value, obj, index) => {
    return `
        <input type="text" id="GdnItemQty_${index}" class="form-control form-control-sm text-right gdn-item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
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

window.tableGdnItemAmount = (value, obj, index) => {
    return `
        <input type="text" id="GdnItemAmt_${index}" class="form-control form-control-sm text-right gdn-item-amount" value="${obj.Amount}" oninput="this.value = _Number.validate({value: this.value, dp: 2, min: 0, max: 999999999999})">
    `;
}

window.tableGdnItemAmountEvent = {
    'input .gdn-item-amount': (e, value, obj, index) => {
        obj.Amount = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
        Table.updateByIndex({ id: '#tableGdnItem', index: index, obj: obj, value: obj.Amount, event: e });
        Gdn.sumOfTotalGdnItem();
    }
}

window.tableGdnReasonCodeDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `GdnReasonCode_${index}`, className: 'gdn-item', data: Gdn.reasonCode, value: 'Value', text: 'Description', initialValue: [obj.ReasonCode], json: true, parent: '.modal' })}
    `;
}

window.tableGdnReasonCodeDescEvent = {
    'change .gdn-item': (e, value, obj, index) => {
        obj.ReasonCode = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            value: obj.ReasonCode,
            event: e
        });
    }
};

window.tableGdnStoreDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({ id: `GdnStore_${index}`, className: 'gdn-item', data: Gdn.store, value: 'Id', text: 'Description', initialValue: [obj.StoreId], json: true, parent: '.modal' })}
    `;
}

window.tableGdnStoreDescEvent = {
    'change .gdn-item': (e, value, obj, index) => {
        obj.StoreId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            value: obj.StoreId,
            event: e
        });
    }
};

window.tableTaskSerialNo = (value, obj, index) => {
    return `<input type="text" id="GdnSerialNo_${index}" class="form-control form-control-sm mb-0 gdn-item" value="${obj.SerialNo}" maxlength="150" />`;
}
window.tableItemUnitDesc = (value, obj, index) => {
    return `<span id="GdnItemUnitDesc_${index}">${obj.UnitDesc ?? ''}</span>`;
}

window.tableItemBalQty = (value, obj, index) => {
    return `<span id="GdnItemBalQty_${index}">${obj.BalQty ?? 0}</span>`;
}
window.tableGdnSerialNoDescEvent = {
    'input .gdn-item': (e, value, obj, index) => {
        obj.SerialNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            value: obj.SerialNo,
            event: e
        });
    }
};

window.tableTaskBatchNo = (value, obj, index) => {
    return `<input type="text" id="GdnBatchNo_${index}" class="form-control form-control-sm mb-0 gdn-item" value="${obj.BatchNo}" maxlength="150" />`;
}

window.tableGdnBatchNoDescEvent = {
    'input .gdn-item': (e, value, obj, index) => {
        obj.BatchNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            value: obj.BatchNo,
            event: e
        });
    }
};

window.tableGdnExpiryOn = (value, obj, index) => {
    return `<input type="date" id="GdnExpiryOn_${index}" class="form-control form-control-sm mb-0 gdn-item" value="${obj.ExpiryOn ?? ""}"/>`;
}

window.tableGdnExpiryOnEvent = {
    'input .gdn-item': (e, value, obj, index) => {
        obj.ExpiryOn = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            value: obj.ExpiryOn,
            event: e
        });
    }
};

window.tableTaskSerialNo = (value, obj, index) => {
    return `<input type="text" id="GdnSerialNo_${index}" 
        class="form-control form-control-sm mb-0 gdn-serial" 
        value="${obj.SerialNo ?? ''}" maxlength="150" />`;
}

window.tableGdnSerialNoDescEvent = {
    'input .gdn-serial': (e, value, obj, index) => {
        obj.SerialNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            value: obj.SerialNo,
            event: e
        });
    }
};


window.tableTaskBatchNo = (value, obj, index) => {
    return `<input type="text" id="GdnBatchNo_${index}" 
        class="form-control form-control-sm mb-0 gdn-batch" 
        value="${obj.BatchNo ?? ''}" maxlength="150" />`;
}

window.tableGdnBatchNoDescEvent = {
    'input .gdn-batch': (e, value, obj, index) => {
        obj.BatchNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableGdnItem',
            index: index,
            obj: obj,
            value: obj.BatchNo,
            event: e
        });
    }
};


window.tableGdnItemQty = (value, obj, index) => {
    return `
        <input type="text" id="GdnItemQty_${index}" 
            class="form-control form-control-sm text-right gdn-item-qty" 
            value="${obj.Qty}" 
            oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
}


window.tableGdnExpiryOn = (value, obj, index) => {

    let val = obj.ExpiryOn
        ? moment(obj.ExpiryOn).format('YYYY-MM-DD')
        : '';

    return `
        <input type="date"
               class="form-control form-control-sm mb-0 gdn-item-expiry"
               value="${val}">
    `;
};


window.tableGdnExpiryOnEvent = {
    'input .Gdn-item': (e, value, obj, index) => {
        obj.ExpiryOn = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.ExpiryOn,
            event: e
        });
    }
};
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
        Gdn.deleteItem({ id: obj.Id, index: index });
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
