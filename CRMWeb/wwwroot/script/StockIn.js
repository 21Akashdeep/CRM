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
        Party.initAdd();
        Party.addOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalParty' });
            StockIn.getAddOption({
                onSuccess: (response) => {
                    Dropdown.bind({ id: '#StockIn-PartyId', data: response.data.Party, value: 'Id', text: 'Description', json: true, initialValue: [obj.Id] });
                    Field.triggerOnChange('#StockIn-PartyId');
                }
            });
        }
        Party.updateOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalParty' });
            setTimeout(() => {
                StockIn.getAddOption({
                    onSuccess: (response) => {
                        Dropdown.bind({ id: '#StockIn-PartyId', data: response.data.Party, value: 'Id', text: 'Description', json: true, initialValue: [obj.Id] });
                        Field.triggerOnChange('#StockIn-PartyId');
                    }
                });
            }, 500);
        }
        $('#StockIn-Scan').on('keydown', (e) => {
            if (e.key == "Enter") {
                try {
                    let obj = JSON.parse($('#StockIn-Scan').val());
                    StockIn.addStockInItem({ ItemId: obj.ItemId, SerialNo: obj.SerialNo, BatchNo: obj.BatchNo, Qty: 1 });
                }
                catch (ex) {
                    Message.error({ statusText: "Scanned QR Code is Invalid." });
                }  
                $('#StockIn-Scan').val('');
            }
        });
        $('#StockInScan-ItemSerialNo').on('keydown', (e) => {

            if (e.key == 'Enter') {

                if (!Field.isMandatory({ class: '.scan-required' })) {
                    return;
                }

                let serialNo = $('#StockInScan-ItemSerialNo').val();

                let countStockInItem =
                    $('#tableStockInItem')
                        .bootstrapTable('getData')
                        .filter(x => x.SerialNo == serialNo).length;

                let countScanItem =
                    $('#tableStockInScanItem')
                        .bootstrapTable('getData')
                        .filter(x => x.SerialNo == serialNo).length;

                if (countStockInItem > 0 || countScanItem > 0) {
                    Message.error({
                        statusText: `Serial No. ${serialNo} already added in Scan List or Item List.`
                    });
                    return;
                }

                StockIn.addStockInItem({
                    itemId: $('#StockInScan-ItemId').val(),
                    itemDesc: $('#StockInScan-ItemId option:selected').text(),
                    expiryOn: $('#StockInScan-ItemExpiryOn').val(),
                    serialNo: serialNo,
                    qty: 1,
                    isScanned: true,
                    callback: (obj) => {
                        Table.add({
                            id: '#tableStockInScanItem',
                            data: obj,
                            action: 'prepend'
                        });
                        $('#StockInScan-ItemSerialNo').val('');
                    }
                });
            }
        });
        $('#btnScanItemAdd').on('click', () => {

            let scanItems =
                $('#tableStockInScanItem').bootstrapTable('getData');

            if (scanItems.length === 0) {
                Message.error({ statusText: 'No scanned items found' });
                return;
            }

            Table.add({
                id: '#tableStockInItem',
                data: scanItems,
                action: 'append',
                selectPick: true
            });

            StockIn.sumOfTotalStockInItem();

            Modal.close({ id: '#modalStockInItemScan' });
        });
        Item.initAdd();
        Item.addOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalItem' });
            StockIn.getAddOption({
                onSuccess: (response) => {
                    StockIn.item = response.data.Item;
                    StockIn.refreshStockInItemDropdowns();
                    Field.triggerOnChange('#StockIn-ItemId');
                }
            });
        };

        Item.updateOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalItem' });

            setTimeout(() => {
                StockIn.getAddOption({
                    onSuccess: (response) => {
                        StockIn.item = response.data.Item;
                        StockIn.refreshStockInItemDropdowns();
                        Field.triggerOnChange('#StockIn-ItemId');
                    }
                });
            }, 500);
        };
        $('#StockIn-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let StockInItem = $('#tableStockInItem').bootstrapTable('getData').filter(x => x.ItemId > 0)
                .map(x => {
                    if (x.ExpiryOn) {
                        x.ExpiryOn = moment(x.ExpiryOn).format('YYYY-MM-DD');
                    }
                    return x;
                });
            let ZeroQtyItem = StockInItem.filter(x => x.Qty == 0);
            if (ZeroQtyItem.length != 0) {
                Message.error({ statusText: 'Qty Zero Input in Any Item!!!' });
                return;
            }
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
                Dropdown.bind({ id: '#StockIn-StockType', data: response.data.StockType, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#StockIn-PartyId', data: response.data.Party, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalStockIn', title: 'StockIn / Add', action: 'Add' });
                $('#StockIn-RefDate,#StockIn-EwayDate').val('');
            }
        });
    }
    static scanner() {
        Modal.open({
            id: '#modalStockInItemScan',
            title: 'StockIn / Scan Item'
        });

        Dropdown.bind({
            id: '#StockInScan-ItemId',
            data: StockIn.item,
            value: 'Id',
            text: 'Description'
        });

        $('#StockInScan-ItemSerialNo').val('');
    }
    static addStockInItem({ itemId = 0, itemDesc = null, serialNo = null, expiryOn = null, qty = 0, isScanned = false, callback } = {}) {
        let obj = {
            Id:0,
            StockInId: 0,
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
            Table.add({id: '#tableStockInItem',data: obj,action: 'append',selectPick: true
            });
            StockIn.sumOfTotalStockInItem();
        }
        else {
            callback(obj);
        }
    }
    static sumOfTotalStockInItem() {
        let StockInItem = $('#tableStockInItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
        let Qty = StockInItem.length == 0 ? 0 : StockInItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);
        let Amount = StockInItem.length == 0 ? 0 : StockInItem.map(x => parseFloat(x.Amount)).reduce((s, v) => s + v, 0);
        let tfoot = `
            <tfoot>
                <tr>
                    <th class="text-right" colspan="5">Total</th>
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
                Dropdown.bind({ id: '#StockIn-StockType', data: option.StockType, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#StockIn-PartyId', data: option.Party, value: 'Id', text: 'Description' });
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
    static deleteItem({ id, index }) {
        Table.remove({ id: '#tableStockInItem', value: [index] });
    }
    static print({ obj }) {
        Data.post({
            url: "StockIn/Print",
            data: obj,
            onSuccess: (response) => {
                if (response.status !== Message.Type.success) {
                    Message.error(response);
                    return;
                }

                let reportTitle = 'Stock In';
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
                    <th>StockIn No.</th>
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

                    // ===== StockIn Items =====
                    let items = obj.VoucherItem || [];

                    table.push(`
            <tr>
                <td colspan="10" style="border-bottom:none;">
                    <table>
                        <thead>
                            <tr>
                                <th colspan="7">StockIn Items</th>
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
    static editItemFromStockIn(item) {

        Item.edit(item.id, 'Edit');
    }
    static refreshStockInItemDropdowns() {
        let rows = $('#tableStockInItem').bootstrapTable('getData');

        rows.forEach((row, index) => {
            Dropdown.bind({
                id: `#StockInItem_${index}`,
                data: StockIn.item,
                value: 'Id',
                text: 'Description',
                json: true,
                initialValue: [row.ItemId]
            });
        });
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
        StockIn.print({ obj: { ListId: [obj.Id] } });
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
        ${Dropdown.html({
            id: `StockInItem_${index}`, className: 'StockIn-item', data: StockIn.item, value: 'Id', text: 'Description', initialValue: [obj.ItemId], json: true, parent: '.modal', search: true,
            size: 4, addFn: 'Item.fill', editFn: 'StockIn.editItemFromStockIn' })}
        
        <input type="text" id="StockInItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 StockIn-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
    `;
}
window.tableStockInExpiryOn = (value, obj) => {
    return `${!obj.ExpiryOn ? '-' : moment(obj.ExpiryOn).format('DD-MM-YYYY')}`;
};
window.tableStockInItemDescEvent = {
    'change .StockIn-item': (e, value, obj, index) => {
        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson?.UnitDesc ?? null;

        obj.Rate = itemJson?.Rate ?? 0;
        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
        obj.StockType = $('#StockIn-StockType').val().toString();
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
window.tableStockInExpiryOn = (value, obj) => {
    return `${!obj.ExpiryOn ? '-' : moment(obj.ExpiryOn).format('DD-MM-YYYY')}`;
};
window.tableStockInItemAction = (value, obj, index) => {
    if(!obj.Id)
    return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableStockInItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        StockIn.deleteItem({ id: obj.Id, index: index });
    }
}
window.tableStockInScanItemDesc = (v, obj) => {
    let item = StockIn.item.find(x => x.Id === obj.ItemId);
    return item?.Description ?? '';
};
window.tableStockInScanExpiry = (v, obj) => {
    return obj.ExpiryOn
        ? moment(obj.ExpiryOn).format('DD-MMM-YYYY')
        : '';
};
window.tableStockInScanQty = (v, obj) => obj.Qty;
window.tableStockInScanSerial = (v, obj) => obj.SerialNo;
window.tableStockInScanItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `
            <button type="button"
                class="btn btn-sm btn-danger rounded-5 btn-delete">
                <span class="fa fa-trash"></span>
            </button>`;
};
window.tableStockInScanItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        Table.remove({
            id: '#tableStockInScanItem',
            value: [index]
        });
    }
};

window.tableStockInItemAction = (value, obj, index) => {
    if (!obj.Id)
        return `
            <button type="button"
                class="btn btn-sm btn-danger rounded-5 btn-delete">
                <span class="fa fa-trash"></span>
            </button>`;
};
window.tableStockInItemActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        Table.remove({
            id: '#tableStockInItem',
            value: [index]
        });
    }
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
