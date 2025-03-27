class Item {
    static init() {
        Item.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });            
            Dropdown.bind({ id: '#ListItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
            Dropdown.bind({ id: '#ListItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', subText: 'SubText' });            
            Dropdown.bind({ id: '#ListId', data: response.data.Item, value: 'Id', text: 'Description', subText: 'SubText' });
        });        
        $('#btnSearch').on('click', () => {
            Item.get({
                ApiName: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableItem', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Item.get({
                ApiName: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableItem', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Item.get({
                ApiName: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Item" });
                }
            });
        });
        $('#btnAdd').on('click', () => { Item.fill(); });
        //Add Item
        Item.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Item/GetViewOption', onSuccess: onSuccess });
    }
    static get({ ApiName, onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListItemGroupId: $('#ListItemGroupId').val(),
            ListItemSubGroupId: $('#ListItemSubGroupId').val(),
            ListId: $('#ListId').val(),
        };
        Data.post(
            {
                url: `Item/${ApiName}`,
                data: obj,
                onSuccess: onSuccess
            }
        );
    }    

    static initAdd() {
        $('#Item_ItemGroupId').on('change', () => {
            if (!Field.isNullOrEmpty($('#Item_ItemGroupId').val())) {
                Item.getAddOption((response) => {                    
                    Dropdown.bind({ id: '#Item_ItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                });
            }
        });        
        ItemSubGroup.initAdd();
        ItemSubGroup.addOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                Modal.close({ id: '#modalItemSubGroup' });
                if (!Field.isNullOrEmpty($('#Item_ItemGroupId').val())) {
                    Dropdown.bind({ id: '#Item_ItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', initialValue: [obj.Id] });
                }
                Dropdown.bind({ id: '#ListItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description' });
            }
        }
        ItemSubGroup.updateOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                Modal.close({ id: '#modalItemSubGroup' });
                if (!Field.isNullOrEmpty($('#Item_ItemGroupId').val())) {
                    Dropdown.bind({ id: '#Item_ItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', initialValue: [obj.Id] });
                }
                Dropdown.bind({ id: '#ListItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description' });                
            }
        }
        ItemGroup.addOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                Modal.close({ id: '#modalItemGroup' });
                Dropdown.bind({ id: '#Item_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', initialValue: [response.obj.Id] });
                Dropdown.bind({ id: '#ListItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description' });
            }
        }
        ItemGroup.updateOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                Modal.close({ id: '#modalItemGroup' });                
                Dropdown.bind({ id: '#Item_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', initialValue: [response.obj.Id] });
                Dropdown.bind({ id: '#ListItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description' });
            }
        }
        //Item Save
        $('#Item_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.item-required' })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formItem" });            
            if (!obj.Id) {
                Item.add(obj);
            }
            else {
                Item.update(obj);
            }
        });
    }
    static getAddOption(onSuccess = () => { }) {   
        let obj = {
            ItemGroupId: $('#Item_ItemGroupId').val() ?? 0
        };
        Data.post({ url: 'Item/GetAddOption', data: obj, onSuccess: onSuccess });
    }
    static fill() {
        Item.getAddOption((response) => {
            Dropdown.bind({ id: '#Item_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
            Dropdown.bind({ id: '#Item_UnitId', data: response.data.Unit, value: 'Id', text: 'Description', subText: 'SubText', json: true });
            Modal.open({ id: '#modalItem', title: 'Item / Add', action: 'add' });
        });
    }    
    static add(obj) {
        Data.post({ url: 'Item/Add', data: obj, onSuccess: Item.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Item.setUiState({ action: "Add", response });
        }
    }
    static edit(id, action = "Edit") {
        Data.get(
            {
                url: `Item/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        Dropdown.bind({ id: '#Item_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                        Dropdown.bind({ id: '#Item_ItemSubGroupId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                        Dropdown.bind({ id: '#Item_UnitId', data: response.data.Unit, value: 'Id', text: 'Description', subText: 'SubText', json: true });
                        let obj = response.obj;
                        let title = action == "Edit" ? `Item / Edit (Code : ${response.obj.Code})` : `Item / Add`;
                        obj.Id = action == "Edit" ? obj.Id : null;
                        Modal.open({ id: '#modalItem', title: title, action: action, obj: obj });
                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );        
    }
    static update(obj) {
        Data.update({ url: 'Item/Update', data: obj, onSuccess: Item.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Item.setUiState({ action: "Update", response });
        }
    }
    static delete(id) {
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
    static deleteOnSuccess(response) {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Item.setUiState({ action: "Delete", response });
        }
    }
    static enable(id) {
        Message.confirm(
            {
                msg: "Do you want to enable???",
                confirmButtonText: "Enable",
                denyButtonText: "Don't Enable",
                data: id,
                onConfirm: (id) => {
                    Data.update(
                        {
                            url: `Item/Enable?Id=${id}`,
                            onSuccess: Item.enableOnSuccess
                        }
                    );
                }
            },
        );
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
}
window.tableItemobjStyle = (obj, index) => {
    if (obj.Status == 0) {
        return { classes: 'text-danger-100' };
    }
    else {
        return { classes: '' };
    }
    console.log(obj);
}
window.tableItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableItemName = (value, obj, index) => {
    return obj.Name.match(/.{1,40}/g).join('<br>');;
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
    let actionBtn = `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ?
                    `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>` :``
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
window.tableItemActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Item.edit(obj.Id, "Edit");
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Item.edit(obj.Id, "Add");
    },
    'click .btn-delete': (e, value, obj, index) => {
        Item.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        Item.enable(obj.Id);
    }
}
