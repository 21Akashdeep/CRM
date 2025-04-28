class Setting {
    static init() {
        Setting.getViewOption();        
        $('#btnAdd').on('click', () => {
            Setting.fill();
        });        
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".required" })) {
                return;
            }
            var obj = Data.serializeToObject({ formId: "#formSetting" });             
            if (Field.isNullOrEmpty(obj.Id)) {
                obj.Id = 0;
                Setting.add(obj);
            }
            else {
                Setting.update(obj);
            }
        });
        $('#btnSearch').on('click', () => {
            Setting.get("Get", (response) => {
                Table.add({ id: '#tableSetting', data: response.data });
            });
        });
        $('#btnPrint').on('click', () => {
            Setting.get("Print", (response) => {
                Table.add({ id: '#tableSetting', data: response.data, reportDesc: 'Setting List', isPrint: true });
            });
        });
        $('#btnExport').on('click', () => {
            Setting.get("Export", (response) => {
                Export.Base64ToExcel({ base64: response.base64, fileName: "Setting" });
            });
        });
    }
    
    static getViewOption() {
        Data.get(
            {
                url: 'Setting/GetViewOption',
                onSuccess: (response) => {                    
                    Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                    Dropdown.bind({ id: '#ListCategory', data: response.data.SettingCategory, value: 'Category', text: 'Category' });
                    Dropdown.bind({ id: '#ListName', data: response.data.SettingName, value: 'Name', text: 'Name' });
                }
            }
        );        
    }    
    static get(method, onSuccess = () => { }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListCategory: $('#ListCategory').val(),
            ListName: $('#ListName').val(),
        };
        Data.post({ url: `Setting/${method}`, data: obj, onSuccess: onSuccess });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Setting/GetAddOption', onSuccess: onSuccess });        
    }
    static fill() {
        Setting.getAddOption((response) => {
            Modal.open({ id: '#modalSetting', title: 'Setting / Add', action: 'Add' });
            Dropdown.bind({ id: '#Category', data: response.data.SettingCategory, value: 'Category', text: 'Category', isEditable: true });
            Dropdown.bind({ id: '#Name', data: response.data.SettingName, value: 'Name', text: ['Name'], isEditable: true });
            Dropdown.bindIcon({ id: '#Icon' });
        });
    }
    static add(obj) {
        Data.post({ url: 'Setting/Add', data: obj, onSuccess: Setting.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalSetting" });
            Table.add({ id: '#tableSetting', data: response.obj, action: 'prepend' });            
            Dropdown.bind({ id: '#ListCategory', data: response.data.SettingCategory, value: 'Category', text: 'Category' });
            Dropdown.bind({ id: '#ListName', data: response.data.SettingName, value: 'Name', text: 'Name' });
            Dropdown.bind({ id: '#Category', data: response.data.SettingCategory, value: 'Category', text: 'Category', isEditable: true });
            Dropdown.bind({ id: '#Name', data: response.data.SettingName, value: 'Name', text: ['Name'], isEditable: true });
        }
    }
    static edit({id, action = "Edit"}) {
        Data.get(
            {
                url: `Setting/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;                        
                        let title = action == "Edit" ? `Setting / ${action} (Name: ${response.obj.Name})` : `Setting / ${action}`;
                        obj.Id = action == "Edit" ? obj.Id : null;                        
                        Modal.open({ id: '#modalSetting', title, action: action, obj: obj });
                        Dropdown.bind({ id: '#Category', data: response.data.SettingCategory, value: 'Category', text: 'Category', isEditable: true });
                        Dropdown.bind({ id: '#Name', data: response.data.SettingName, value: 'Name', text: ['Name'], isEditable: true });                        
                        $('#Category').val(obj.Category);
                        $('#Name').val(obj.Name);
                        Dropdown.bindIcon({ id: '#Icon', value: [obj.Icon] });
                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
        
    }
    static update(obj) {
        Data.update({ url: `Setting/Update`, data: obj, onSuccess: Setting.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalSetting" });
            Table.updateById({ id: '#tableSetting', objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListCategory', data: response.data.SettingCategory, value: 'Category', text: 'Category' });
            Dropdown.bind({ id: '#ListName', data: response.data.SettingName, value: 'Name', text: 'Name' });
        }
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
                            url: `Setting/Delete?Id=${id}`,
                            onSuccess: Setting.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {            
            Table.updateById({ id: '#tableSetting', objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListCategory', data: response.data.SettingCategory, value: 'Category', text: 'Category' });
            Dropdown.bind({ id: '#ListName', data: response.data.SettingName, value: 'Name', text: 'Name' });
        }
    }
    static enable(id) {
        Message.confirm(
            {
                msg: "Do you want to delete???",
                confirmButtonText: "Enable",
                denyButtonText: "Don't Enable",
                data: id,
                onConfirm: (id) => {
                    Data.update(
                        {
                            url: `Setting/Enable?Id=${id}`,
                            onSuccess: Setting.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: '#tableSetting', objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListCategory', data: response.data.SettingCategory, value: 'Category', text: 'Category' });
            Dropdown.bind({ id: '#ListName', data: response.data.SettingName, value: 'Name', text: 'Name' });
        }
    }
}

tableSettingSLNoFormatter = (value, row, index) => {
    return index + 1;
}
tableSettingStatusFormatter = (value, row, index) => {
    return '<div class="' + row.StatusCss + '">' + row.StatusText + '</div>';
}
tableSettingCreatedByAndAtFormatter = (value, row, index) =>{
    return row.CreatedByName + "<br>" + moment(row.CreatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
tableSettingUpdatedByAndAtFormatter = (value, row, index) => {
    return row.UpdatedByName + "<br>" + moment(row.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
tableSettingActionFormatter = (value, obj, index) => {
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
window.tableSettingActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Setting.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Setting.edit({ id: obj.Id, action: "Add" });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Setting.delete({id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Setting.enable(obj.Id);
    }
}