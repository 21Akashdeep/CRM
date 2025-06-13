class AdminDiv {
    //AdminDiv List
    static init() {
        AdminDiv.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.AdminDiv, value: 'Id', text: 'Description', subText: "Code" });
        });
        $('#btnSearch').on('click', () => {
            AdminDiv.get({
                action: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableAdminDiv', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            AdminDiv.get({
                action: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableAdminDiv', data: response.data, isPrint: true, reportDesc: 'AdminDiv List' });
                }
            });
        });
        $('#btnExport').on('click', () => {
            AdminDiv.get({
                action: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "AdminDiv" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            AdminDiv.fill();
        });
        AdminDiv.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'AdminDiv/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post({ url: `AdminDiv/${action}`, data: obj, onSuccess: onSuccess });
    }

    //AdminDiv Add
    static initAdd() {
        $('#AdminDiv_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".admindiv-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formAdminDiv" });
            if (!obj.Id) {
                AdminDiv.add(obj);
            }
            else {
                AdminDiv.update(obj);
            }
        });

    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'AdminDiv/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        AdminDiv.getAddOption((response) => {
            Modal.open({ id: '#modalAdminDiv', title: 'AdminDiv / Add', action: 'Add' });
            $('#AdminDiv_SeqNo').val(0);
            Dropdown.bind({ id: '#AdminDiv_Country', data: response.data.Country, value: 'Value', text: 'Description' });

        });
    }
    static add(obj) {
        Data.post(
            {
                url: 'AdminDiv/Add',
                data: obj,
                onSuccess: AdminDiv.addOnSuccess
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalAdminDiv" });
            Table.add({ id: "#tableAdminDiv", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.AdminDiv, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `AdminDiv/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `AdminDiv / Edit (Code: ${obj.Code})` : `AdminDiv / Add`;
                        Modal.open({ id: '#modalAdminDiv', title: title, action: action, obj: obj });
                        Dropdown.bind({ id: '#AdminDiv_Country', data: response.data.Country, value: 'Value', text: 'Description' });
                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update(
            {
                url: 'AdminDiv/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    AdminDiv.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalAdminDiv" });
            Table.updateById({ id: "#tableAdminDiv", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.AdminDiv, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static delete({ id }) {
        Message.confirm(
            {
                msg: 'Do you want to delete??',
                confirmButtonText: 'Delete',
                denyButtonText: 'Dont Delete',
                data: id,
                onConfirm: (id) => {
                    Data.delete({ url: `AdminDiv/Delete?Id=${id}`, onSuccess: AdminDiv.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableAdminDiv", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.AdminDiv, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        }
    }
    static enable({ id }) {
        Message.confirm(
            {
                msg: 'Do you want to enable??',
                confirmButtonText: 'Enable',
                denyButtonText: 'Dont Enable',
                data: id,
                onConfirm: (id) => {
                    Data.update({ url: `AdminDiv/Enable?Id=${id}`, onSuccess: AdminDiv.enableOnSuccess });

                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableAdminDiv", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.AdminDiv, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        }
    }
}
window.tableAdminDivSLNo = (value, obj, index) => {
    return index + 1;
}
tableAdminDivStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}<div>`;
}
tableAdminDivCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
tableAdminDivUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
tableAdminDivAction = (value, obj, index) => {
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
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableAdminDivActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        AdminDiv.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        AdminDiv.edit({ id: obj.Id, action: 'Add' })
    },
    'click .btn-delete': (e, value, obj, index) => {
        AdminDiv.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        AdminDiv.enable({ id: obj.Id });
    }
}
