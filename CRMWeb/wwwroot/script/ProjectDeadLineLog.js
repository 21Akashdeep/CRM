class ProjectDeadLineLog {
    static init() {
        ProjectDeadLineLog.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ProjectDesc', data: response.data.Project, value: 'Id', text: 'Description' });

        });
        $('#btnSearch').on('click', () => {
            ProjectDeadLineLog.get({
                action: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableProjectDeadLineLog', data: response.data })
                }
            });
        });
        $('#btnPrint').on('click', () => {
            ProjectDeadLineLog.get({
                action: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableProjectDeadLineLog', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            ProjectDeadLineLog.get({
                action: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "ProjectDeadLineLog" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            ProjectDeadLineLog.fill();
        });
        ProjectDeadLineLog.initAdd();

    }
    //ProjectDeadLineLog View
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'ProjectDeadLineLog/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post({ url: `ProjectDeadLineLog/${action}`, data: obj, onSuccess: onSuccess });
    }
    //ProjectDeadLineLog Add, Edit, Update & Delete
    static initAdd() {
        $('#ProjectDeadLineLog_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".ProjectDeadLineLog-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formProjectDeadLineLog" });
            if (!obj.Id) {
                ProjectDeadLineLog.add(obj);
            }
            else {
                ProjectDeadLineLog.update(obj);
            }
        });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'ProjectDeadLineLog/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Modal.open({ id: '#modalProjectDeadLineLog', title: 'ProjectDeadLineLog / Add', action: 'add' });
        $('#Customer_SeqNo').val(0);
    }
    static add(obj) {
        console.log("akash")
        Data.post({ url: 'ProjectDeadLineLog/Add', data: obj, onSuccess: ProjectDeadLineLog.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalProjectDeadLineLog" });
            Table.add({ id: "#tableProjectDeadLineLog", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `ProjectDeadLineLog/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `ProjectDeadLineLog / Edit (Code: ${obj.Code})` : `ProjectDeadLineLog / Add`;
                        Modal.open({ id: '#modalProjectDeadLineLog', title: title, action: action, obj: obj });
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
                url: 'ProjectDeadLineLog/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    ProjectDeadLineLog.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalProjectDeadLineLog" });
            Table.updateById({ id: "#tableProjectDeadLineLog", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
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
                            url: `ProjectDeadLineLog/Delete?Id=${id}`,
                            onSuccess: ProjectDeadLineLog.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableProjectDeadLineLog", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }
    static enable({ id }) {
        Message.confirm(
            {
                msg: "Do you want to enable???",
                confirmButtonText: "Enable",
                denyButtonText: "Don't Enable",
                data: id,
                onConfirm: (id) => {
                    Data.update(
                        {
                            url: `ProjectDeadLineLog/Enable?Id=${id}`,
                            onSuccess: ProjectDeadLineLog.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableProjectDeadLineLog", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }

}
window.tableProjectDeadLineLogSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableProjectDeadLineLogStatus = (value, obj, index) => { 
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableProjectDeadLineDate = (value, obj, index) => {
    return `<div>${moment(obj.DeadLineDate).format('DD-MMM-YYYY')}</div>`;
}
window.tableProjectDeadLineLogCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableProjectDeadLineLogUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableProjectDeadLineLogAction = (value, obj, index) => {
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
window.tableProjectDeadLineLogActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        ProjectDeadLineLog.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        ProjectDeadLineLog.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        ProjectDeadLineLog.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        ProjectDeadLineLog.enable({ id: obj.Id });
    }
}