class ItemSubGroup {
    static init() {
        ItemSubGroup.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ItemSubGroup, value: 'Id', text: 'Description', subText: 'SubText' });
            Dropdown.bind({ id: '#ListItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
        });
        $('#btnSearch').on('click', () => {
            ItemSubGroup.get({
                ApiName: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableItemSubGroup', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            ItemSubGroup.get({
                ApiName: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableItemSubGroup', data: response.data, isPrint });
                }
            });
        });
        $('#btnExport').on('click', () => {
            ItemSubGroup.get({
                ApiName: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "ItemSubGroup" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            ItemSubGroup.fill();
        });
        ItemSubGroup.initAdd();        
    }
    static getViewOption(onSuccess) {
        Data.get({ url: 'ItemSubGroup/GetViewOption', onSuccess: onSuccess });
    }
    static get({ ApiName, onSuccess}) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val(),
            ListItemGroupId: $('#ListItemGroupId').val(),
        };
        Data.post({ url: `ItemSubGroup/${ApiName}`, data: obj, onSuccess: onSuccess });
    }
    
    static initAdd() {        
        ItemGroup.initAdd();
        ItemGroup.addOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                let obj = response.obj;
                Modal.close({ id: '#modalItemGroup' });
                ItemSubGroup.getAddOption({
                    onSuccess: (response) => {
                        Dropdown.bind({ id: '#ItemSubGroup_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText', initialValue: [obj.Id] });
                    }
                });                
            }
        }
        ItemGroup.updateOnSuccess = (response) => {
            Message.show(response);
            if (response.status == Message.Type.success) {
                let obj = response.obj;
                Modal.close({ id: '#modalItemGroup' });
                ItemSubGroup.getAddOption({
                    onSuccess: (response) => {
                        Dropdown.bind({ id: '#ItemSubGroup_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText', initialValue: [obj.Id] });
                    }
                });
            }
        }        
        $('#ItemSubGroup_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".item-sub-group-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formItemSubGroup" });
            if (Field.isNullOrEmpty(obj.Id)) {
                ItemSubGroup.add(obj);
            }
            else {
                ItemSubGroup.update(obj);
            }
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'ItemSubGroup/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        ItemSubGroup.getAddOption({
            onSuccess: (response) => {
                Modal.open({ id: '#modalItemSubGroup', title: 'Item Sub Group / Add', action: 'add' });
                Dropdown.bind({ id: '#ItemSubGroup_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
            }
        });        
    }
    static add(obj) {
        Data.post({ url: 'ItemSubGroup/Add', data: obj, onSuccess: ItemSubGroup.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            ItemSubGroup.setUiState({ action: 'Add', response: response });
        }
    }
    static edit(id, action = "Edit") {
        Data.get(
            {
                url: `ItemSubGroup/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        Dropdown.bind({ id: '#ItemSubGroup_ItemGroupId', data: response.data.ItemGroup, value: 'Id', text: 'Description', subText: 'SubText' });
                        let title = action == "Edit" ? `Item Sub Group / Edit (Code : ${response.obj.Code})` : `Item Sub Group / Add`;
                        response.obj.Id = action == "Edit" ? response.obj.Id : null;
                        Modal.open({ id: '#modalItemSubGroup', title: title, action: action, obj: response.obj });
                    }
                    else {
                        Message.alert(response);
                    }
                }
            }
        );
    }    
    static update(obj) {
        Data.update({ url: 'ItemSubGroup/Update', data: obj, onSuccess: ItemSubGroup.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            ItemSubGroup.setUiState({ action: 'Update', response: response });
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
                            url: `ItemSubGroup/Delete?Id=${id}`,
                            onSuccess: ItemSubGroup.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            ItemSubGroup.setUiState({ action: 'Delete', response: response });
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
                            url: `ItemSubGroup/Enable?Id=${id}`,
                            onSuccess: ItemSubGroup.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            ItemSubGroup.setUiState({ action: 'Enable', response: response });
        }
    }
    static setUiState({ action = null, response }) {
        switch (action) {
            case "Add":
                Modal.reset({ id: '#modalItemSubGroup' });
                Table.add({ id: "#tableItemSubGroup", data: response.obj, action: 'prepend' });
                break;
            case "Update":
                Modal.close({ id: '#modalItemSubGroup' });
                Table.updateById({ id: "#tableItemSubGroup", objId: response.obj.Id, obj: response.obj });
                break;
            default:
                Table.updateById({ id: "#tableItemSubGroup", objId: response.obj.Id, obj: response.obj });
                break;
        }
        Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        Dropdown.bind({ id: '#ListId', data: response.data.Unit, value: 'Id', text: 'Description', subText: 'Name' });
    }
}
window.tableItemSubGroupSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableItemSubGroupStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}</div>`;
}
window.tableItemSubGroupCreatedBy = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableItemSubGroupUpdatedBy = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableItemSubGroupAction = (value, obj, index) => {
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
window.tableItemSubGroupActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        ItemSubGroup.edit(obj.Id);
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        ItemSubGroup.edit(obj.Id);
    },
    'click .btn-delete': (e, value, obj, index) => {
        ItemSubGroup.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        ItemSubGroup.enable(obj.Id);
    }
}