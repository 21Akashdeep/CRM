class ProjectModule {
    static init() {
        ProjectModule.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ProjectDesc', data: response.data.Project, value: 'Id', text: 'Description' });
        });

        $('#btnSearch').on('click', () => {
            ProjectModule.get({
                action: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableProjectModule', data: response.data })
                }
            });
        });
        $('#btnPrint').on('click', () => {
            ProjectModule.get({
                action: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableProjectModule', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            ProjectModule.get({
                action: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "ProjectModule" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            ProjectModule.fill();
        });
        ProjectModule.initAdd();

    }
    //ProjectModule View
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'ProjectModule/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val(),
            ListProjectId: $('#ProjectDesc').val()

        }
        Data.post({ url: `ProjectModule/${action}`, data: obj, onSuccess: onSuccess });
    }
    //ProjectModule Add, Edit, Update & Delete
    static initAdd() {   
        $('#ProjectModule_btnSave').on('click', async () => {
            if (!Field.isMandatory({ class: ".ProjectModule-required" })) {
                return;
            }

            let obj = Data.serializeToObject({ formId: "#formProjectModule" });
            obj.Document = await _File.info(document.getElementById('TechnicalDoc'));

            if (!obj.Id) {
                ProjectModule.add(obj);
            } else {
                ProjectModule.update(obj);
            }
        });

        $('#TechnicalDoc').on('change', (e) => {
            _File.attach({
                fileId: `#${e.target.id}`,
                iframeId: `#TechDocPreview`
            });
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'ProjectModule/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        ProjectModule.getAddOption({
            onSuccess: (response) => {
                Modal.open({ id: '#modalProjectModule', title: 'Project / Add', action: 'add' });
                Dropdown.bind({ id: '#ProjectModule-ProjectDesc', data: response.data.Project, value: 'Id', text: 'Description', subText: 'SubText' });
            }
        });
    }
    static add(obj) {
        console.log("akash")
        Data.post({ url: 'ProjectModule/Add', data: obj, onSuccess: ProjectModule.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalProjectModule" });
            Table.add({ id: "#tableProjectModule", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `ProjectModule/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        Dropdown.bind({ id: '#ProjectModule-ProjectDesc', data: response.data.Project, value: 'Id', text: 'Description', subText: 'SubText'});
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `ProjectModule / Edit (Code: ${obj.Code})` : `ProjectModule / Add`;
                       
                        Modal.open({ id: '#modalProjectModule', title: title, action: action, obj: obj });

                        if (action == "Edit") {
                            let TechDoc = response.data.TechDoc;
                            let blobUrl = _File.blobUrl({ base64: TechDoc.Base64, mimeType: TechDoc.MimeType });
                            _File.setIframe({ iframeId: '#TechDocPreview', mimeType: TechDoc.MimeType, src: blobUrl });
                        }
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
                url: 'ProjectModule/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    ProjectModule.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalProjectModule" });
            Table.updateById({ id: "#tableProjectModule", objId: response.obj.Id, obj: response.obj });
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
                            url: `ProjectModule/Delete?Id=${id}`,
                            onSuccess: ProjectModule.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableProjectModule", objId: response.obj.Id, obj: response.obj });
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
                            url: `ProjectModule/Enable?Id=${id}`,
                            onSuccess: ProjectModule.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableProjectModule", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }
    static getFile({ filePath = "", onSuccess }) {
        Data.get({
            url: `ProjectModule/GetFile?FilePath=${filePath}`,
            onSuccess: (response) => {
                if (response.status == Message.Type.error) {
                    Message.show(response);
                    return;
                }
                onSuccess(response);
            }
        });
    }

}
window.tableProjectModuleSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableProjectModuleStatus = (value, obj, index) => { 
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableProjectDeadLineDate = (value, obj, index) => {
    return `<div>${moment(obj.DeadLineDate).format('DD-MMM-YYYY')}</div>`;
}
window.tableProjectModuleCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableProjectModuleUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableProjectModuleAction = (value, obj, index) => {
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
window.tableProjectModuleActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        ProjectModule.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        ProjectModule.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        ProjectModule.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        ProjectModule.enable({ id: obj.Id });
    }
}