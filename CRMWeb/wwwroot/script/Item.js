class Item {
    static unit = [];
    static baseUnit = [];
    static init() {
        Item.getViewOption();
        $('#btnSearch').on('click', () => {
            Item.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableItem', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Item.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableItem', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Item.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: 'Item' });
                }
            });
        });
        $('#btnNewEntry').on('click', () => {
            Item.newEntry();
        });
        $('#Item_ItemGroupId').off('change').on('change', function () {
            if ($('#Item_ItemGroupId').val() != null) {
                Item.getAddOption({
                    onSuccess: (response) => {
                        Dropdown.bind({
                            id: '#Item_ItemSubGroupId',
                            data: response.data.ItemSubGroup,
                            value: 'Id',
                            text: 'Description'

                        });
                    }
                });
            }
            else {
                Dropdown.bind({ id: '#Item-ItemSubGroupId', data: [], value: 'Id', text: 'Description' });
            }
        });
        $('#Item_UnitId').on('change', () => {
            let baseUnit = $('#Item_UnitId').val();
            let obj = {
                Id: 0,
                UnitId: baseUnit,
                UnitDesc: null,
                ValuePerUnit: 1,
                ConversionFactor: 1,
                IsBaseUnit: true,
                IsSmallest: false,
            }
            Item.baseUnit = obj;
            Table.add({ id: '#tableItemUnit', data: [] });
        });

        $('#Item_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.item-required' })) {
                return;
            }
            let ItemUnit = $('#tableItemUnit').bootstrapTable('getData');
            ItemUnit = ItemUnit.concat(Item.baseUnit);
            let obj = Data.serializeToObject({ formId: "#formItem" });
            obj.ItemUnit = ItemUnit;
            if (!obj.Id) {
                Item.add(obj);
            }
            else {
                Item.update(obj);
            }
        });
    }

    static getViewOption() {
        Data.get({
            url: 'Item/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                Dropdown.bind({ id: '#ListItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                Dropdown.bind({ id: '#ListId', data: response.data.Item, value: 'Id', text: 'Description', subText: 'SubText' });
            }
        });
    }
    static getAddOption({ onSuccess }) {
        let obj = {

            ItemGroupId: $('#Item_ItemGroupId').val() || 0
        }
        Data.post({ url: 'Item/GetAddOption', data: obj, onSuccess: onSuccess });
    }
    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListItemGroupId: $('#ListItemGroupId').val(),
            ListItemSubGroupId: $('#ListItemSubGroupId').val(),
            ListId: $('#ListId').val(),
        };
        Data.post({
            url: `Item/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static newEntry() {
        Item.getAddOption({
            onSuccess: (response) => {
                Item.unit = response.data.Unit;
                Dropdown.bind({ id: '#Item_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                Dropdown.bind({ id: '#Item_ItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                Dropdown.bind({ id: '#Item_UnitId', data: response.data.Unit, value: 'UnitId', text: 'UnitDesc', subText: 'SubText' });
                Modal.open({ id: '#modalItem', title: 'Item / Add', action: 'Add' });               
            }
        });
    }
    static addItemUnit() {
        if (!$('#Item_UnitId').val()) {
            Message.error({ statusText: 'Base Unit is not selected.' });
            return;
        }

        let obj = {
            Id: 0,
            ItemId: 0,
            UnitId: 0,
            UnitDesc: null,
            ValuePerUnit: 0,
            ConversionFactor: 0,
            IsBaseUnit: false,
            IsSmallest: false,

        };
        Table.add({ id: '#tableItemUnit', data: obj, action: 'append', selectPick: true });
    }      
    static add(obj) {
        Data.post({
            url: 'Item/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalItem' });
                    Table.add({ id: '#tableItem', data: response.obj, action: 'prepend' });
                    Dropdown.bind({ id: '#ListId', data: response.data.ViewOption.Item, value: 'Id', text: 'ItemNo', subText: 'Date' });

                }
            }
        });
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `Item/Edit?Id=${id}`,
            onSuccess: (response) => {
                let option = response.data;               
                let obj = response.obj;
                Item.unit = response.data.Unit;
                let baseUnit = obj.ItemUnit.filter(x => x.IsBaseUnit == true);
                let notBaseUnit = obj.ItemUnit.filter(x => x.IsBaseUnit == false);
                let baseunitId = baseUnit.find(x => x.IsBaseUnit === true)?.UnitId;
                obj.UnitId = baseunitId;
                obj.Id = action == 'Edit' ? obj.Id : null;
                let title = action === 'Edit' ? `Item / Edit (Code: ${obj.No})` : `Item / Add`;
                
                Dropdown.bind({ id: '#Item_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                Dropdown.bind({ id: '#Item_ItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                Dropdown.bind({ id: '#Item_UnitId', data: response.data.Unit, value: 'UnitId', text: 'UnitDesc', subText: 'SubText' });

                Modal.open({ id: '#modalItem', title: title, action: action, obj: obj });
                +Table.add({ id: '#tableItemUnit', data: notBaseUnit, selectPick: true });
                Item.sumOfTotalItemItem();
            }
        });
    }
    static update(obj) {
        Data.update({
            url: 'Item/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalItem' });
                    Table.updateById({ id: '#tableItem', objId: response.obj.Id, obj: response.obj });
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
                            url: `Item/Delete?Id=${id}`,
                            onSuccess: Item.deleteOnSuccess
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
        Table.remove({ id: '#tableItemItem', value: [index] });
    }
    static deleteScanItem({ id, index }) {
        Table.remove({ id: '#tableItemScanItem', value: [index] });
    }
    static enableOnSuccess(response) {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Item.setUiState({ action: "Enable", response });
        }
    }
    static setUiState({ action = null, response }) {
        switch (action) {
            case "Add":
                Modal.reset({ id: '#modalItem' });
                Table.add({ id: "#tableItem", data: response.obj, action: 'prepend' });
                break;
            case "Update":
                Modal.close({ id: '#modalItem' });
                Table.updateById({ id: "#tableItem", objId: response.obj.Id, obj: response.obj });
                break;
            default:
                Table.updateById({ id: "#tableItem", objId: response.obj.Id, obj: response.obj });
                break;
        }
        Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        Dropdown.bind({ id: '#ListId', data: response.data.Item, value: 'Id', text: 'Description', subText: 'Name' });
        Dropdown.bind({ id: '#ListItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'Name' });
        Dropdown.bind({ id: '#ListItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', subText: 'Name' });
    }
    static deleteItemUnit({ id, index }) {
        Table.remove({ id: '#tableItemUnit', value: [index] });
    }


    static print({ obj }) {
        Data.post({
            url: "Item/Print",
            data: obj,
            onSuccess: (response) => {
                if (response.status !== Message.Type.success) {
                    Message.error(response);
                    return;
                }

                let reportTitle = 'Receipt Notes';
                if (obj.FromDate && obj.TillDate) {
                    reportTitle += ` ${moment(obj.FromDate).format('DD-MMM-YYYY')} To ${moment(obj.TillDate).format('DD-MMM-YYYY')}`;
                }

                let printContent = [];
                let ItemList = Array.isArray(response.data) ? response.data : [];

                ItemList.forEach((obj, index) => {
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
                        <th>Item No.</th>
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

                    // ===== Item Master Row =====
                    table.push(`
                <tr>
                    <td class="text-center">${index + 1}</td>
                    <td>${obj.No}</td>
                    <td>${moment(obj.Date).format('DD-MMM-YYYY')}</td>
                    <td>${obj.StoreDesc || '-'}</td>
                    <td>${obj.PartyDesc || '-'}</td>
                    <td>${obj.ChallanNo || '-'}</td>                
                    <td>${obj.Remarks || '-'}</td>
                    <td>${obj.CreatedByName}</td>
                </tr>
                `);

                    // ===== Item Items =====
                    let items = obj.VoucherItem || [];

                    table.push(`
                <tr>
                    <td colspan="10" style="border-bottom:none;">
                        <table>
                            <thead>
                                <tr>
                                    <th colspan="7">Item Items</th>
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
    static editItemFromItem(item) {

        Item.edit(item.id, 'Edit');
    }
    static refreshItemItemDropdowns() {
        let rows = $('#tableItemItem').bootstrapTable('getData');

        rows.forEach((row, index) => {
            Dropdown.bind({
                id: `#ItemItem_${index}`,
                data: Item.item,
                value: 'Id',
                text: 'Description',
                json: true,
                initialValue: [row.ItemId]
            });
        });
    }
}
//Table Item
window.tableItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableItemDate = (value, obj, index) => {
    return !value ? '-' : moment(value).format('DD-MMM-YYYY');
}
window.tableItemStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableItemName = (value, obj, index) => {
    return `<div>Name: ${obj.Name.match(/.{1,40}/g).join('<br>')}</div>
            <div>Make: ${obj.Make ? obj.Make : "-"}</div>
            <div>Model: ${obj.Model ? obj.Model : "-"}</div>
       `;
}
window.tableItemDescription = (value, obj, index) => {
    return obj.Description.match(/.{1,40}/g).join('<br>');
}
window.tableItemStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableItemCreatedBy = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableItemUpdatedBy = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableItemAction = (value, obj, index) => {
    let actionBtn = [];
    if (obj.IsEdit) {
        actionBtn.push(`<li>
                        <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>`);
    }
    if (obj.IsDuplicate) {
        actionBtn.push(`<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-duplicate" title="Duplicate">
                            <span class="fa fa-copy text-primary-100"></span>&nbsp;&nbsp;Duplicate
                        </a>
                    </li>`);
    }
    if (obj.IsDelete) {
        actionBtn.push(`<li>
                        <a href="#" class="dropdown-item text-danger-100 btn-delete" title="Delete">
                            <span class="fa fa-trash text-danger-100"></span>&nbsp;&nbsp;Delete
                        </a>
                    </li>`);
    }
    if (obj.IsEnable) {
        actionBtn.push(`<li>
                        <a href="#" class="dropdown-item text-success-100 btn-enable" title="Enable">
                            <span class="fa fa-toggle-on text-success-100"></span>&nbsp;&nbsp;Enable
                        </a>
                    </li>`);
    }
    if (obj.IsPrint) {
        actionBtn.push(`<li>
                <a href="#" class="dropdown-item text-primary-100 btn-print" title="Print">
                    <span class="fa fa-print text-primary-100"></span>&nbsp;&nbsp;Print
                </a>
            </li>`);
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
window.tableItemActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Item.edit({ id: obj.Id, action: 'Edit' });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Item.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        Item.print({ obj: { ListId: [obj.Id] } });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Item.delete({ id: obj.Id });
    }
}


//TableItemUnit
window.tableItemUnitSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableItemUnitDesc = (value, obj, index) => {
    let itemUnit = JSON.parse(JSON.stringify(Item.unit));
    itemUnit = itemUnit.filter(x => x.UnitId != $('#Item_UnitId').val());
    return `
        <div style="min-width:400px">
        ${Dropdown.html({ id: `ItemUnit_${index}`, className: 'item-unit', data: itemUnit, value: 'UnitId', text: 'UnitDesc', initialValue: [obj.UnitId], json: true, parent: '.modal' })}
        </div>
        
    `;
}
window.tableItemUnitDescEvent = {
    'change .item-unit': (e, value, obj, index) => {
        obj.UnitId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
        //Set Unit & Rate
        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
        obj.UnitDesc = itemJson?.UnitDesc ?? null;        
        Table.updateByIndex({ id: '#tableItemUnit', index: index, obj: obj, value: obj.UnitId, event: e });        
    }
}
window.tableItemUnitValuePerUnit = (value, obj, index) => {
    return `
        <div class="input-group">            
            <input type="text" id="ItemValuePerUnit_${index}" class="form-control form-control-sm text-end mb-0 item-valueperunit" value="${obj.ValuePerUnit}"
            oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})"/>
        </div>
    `;
}
window.tableItemUnitValuePerUnitEvent = {
    'input .item-valueperunit': (e, value, obj, index) => {
        obj.ValuePerUnit = e.currentTarget.value;        
        obj.ConversionFactor = 1 / obj.ValuePerUnit;
        Table.updateByIndex({ id: '#tableItemUnit', index: index, obj: obj, value: obj.ValuePerUnit, event: e });
    }
};

window.tableItemUnitAction = (value, obj, index) => {
    if (!obj.Id)
        return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableItemUnitActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        Item.deleteItemUnit({ id: obj.Id, index: index });
    }
}


