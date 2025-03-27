class ItemGroup {
    static init() {
        ItemGroup.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ItemGroup, value: 'Id', text: 'Name', subText: 'Description' });
        });
        $('#btnSearch').on('click', () => {
            ItemGroup.get({
                action: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableItemGroup', data: response.data })
                }
            });
        });
        $('#btnPrint').on('click', () => {
            ItemGroup.get({
                action: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableItemGroup', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            ItemGroup.get({
                action: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "ItemGroup" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            ItemGroup.fill();
        });
        ItemGroup.initAdd();
        
    }
    //Item Group View
    static getViewOption(onSuccess) {
        Data.get({ url: 'ItemGroup/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val(),
            ListUQCG: $('#ListUQCG').val(),
        };
        Data.post({ url: `ItemGroup/${action}`, data: obj, onSuccess: onSuccess });
    }
    //Item Group Add, Edit, Update & Delete
    static initAdd() {
        $('#ItemGroup_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".item-group-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formItemGroup" });
            if (Field.isNullOrEmpty(obj.Id)) {
                ItemGroup.add(obj);
            }
            else {
                ItemGroup.update(obj);
            }
        });
    }
    static fill() {
        Modal.open({ id: '#modalItemGroup', title: 'Item Group / Add', action: 'add' });
    }
    static add(obj) {
        Data.post({ url: 'ItemGroup/Add', data: obj, onSuccess: ItemGroup.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            ItemGroup.updateUiState({ action: 'Add', response: response });
        }
    }
    static edit(id, action = "Edit") {
        Data.get(
            {
                url: `ItemGroup/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let title = action == "Edit" ? `Item Group / Edit (Code : ${response.obj.Code})` : `Item Group / Add`;
                        response.obj.Id = action == "Edit" ? response.obj.Id : null;
                        Modal.open({ id: '#modalItemGroup', title: title, action: action, obj: response.obj });
                    }
                    else {
                        Message.alert(response);
                    }
                }
            }
        );
    }    
    static update(obj) {
        Data.update({ url: 'ItemGroup/Update', data: obj, onSuccess: ItemGroup.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            ItemGroup.updateUiState({ response: response });
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
                            url: `ItemGroup/Delete?Id=${id}`,
                            onSuccess: ItemGroup.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            ItemGroup.updateUiState({ response: response });
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
                            url: `ItemGroup/Enable?Id=${id}`,
                            onSuccess: ItemGroup.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            ItemGroup.updateUiState({ response: response });
        }
    }
    static updateUiState({ action = null, response }) {
        switch (action) {
            case "Add":
                Modal.reset({ id: '#modaItemGroup' });
                Table.add({ id: "#tableItemGroup", data: response.obj, action: 'prepend' });
                break;
            default:
                Modal.close({ id: '#modaItemGroup' });
                Table.updateById({ id: "#tableItemGroup", objId: response.obj.Id, obj: response.obj });
                break;
        }
        Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        Dropdown.bind({ id: '#ListId', data: response.data.Unit, value: 'Id', text: 'Description', subText: 'Name' });        
    }
}
window.tableItemGroupSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableItemGroupStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableItemGroupCreatedBy = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableItemGroupUpdatedBy = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableItemGroupAction = (value, obj, index) => {
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
window.tableItemGroupActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        ItemGroup.edit(obj.Id);
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        ItemGroup.edit(obj.Id, "Add");
    },
    'click .btn-delete': (e, value, obj, index) => {
        ItemGroup.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        ItemGroup.enable(obj.Id);
    }
}