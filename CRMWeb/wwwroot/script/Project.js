class Project {
    static init() {
        Project.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#Project_CustomerId', data: response.data.Customer, value: 'Id', text: 'Description' });
        });
        $('#btnSearch').on('click', () => {
            Project.get({
                action: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableProject', data: response.data })
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Project.get({
                action: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableProject', data: response.data, isPrint: true });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Project.get({
                action: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Project" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            Project.fill();
        });
        Project.initAdd();

    }
    //Project View
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Project/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post({ url: `Project/${action}`, data: obj, onSuccess: onSuccess });
    }
    //Project Add, Edit, Update & Delete
    static initAdd() {
        $('#Project_btnSave').on('click', async () => {
            if (!Field.isMandatory({ class: ".Project-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formProject" });
            obj.Document = await _File.info(document.getElementById('AttachTechDoc'));

            console.log(obj)
            if (!obj.Id) {
                Project.add(obj);
            }
            else {
                Project.update(obj);
            }
        });
        $('#AttachTechDoc').on('change', (e) => {
            _File.attach({
                fileId: `#${e.target.id}`,
                iframeId: `#TechDocPreview`
            });
        });
        //$('#btnViewPersonId').on('click', () => {
        //    let filePath = $('#btnViewPersonId').attr('data-value')
        //    if (filePath) {
        //        OutPass.getFile(filePath);
        //    }
        //});
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Project/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Modal.open({ id: '#modalProject', title: 'Project / Add', action: 'add' });
        $('#PoDate').val('');
    }
    static add(obj) {
        console.log("akash")
        Data.post({ url: 'Project/Add', data: obj, onSuccess: Project.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalProject" });
            Table.add({ id: "#tableProject", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `Project/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Project / Edit (Code: ${obj.Code})` : `Project / Add`;
                        Modal.open({ id: '#modalProject', title: title, action: action, obj: obj });

                        //Set PersonId Data
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
                url: 'Project/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    Project.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalProject" });
            Table.updateById({ id: "#tableProject", objId: response.obj.Id, obj: response.obj });
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
                            url: `Project/Delete?Id=${id}`,
                            onSuccess: Project.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableProject", objId: response.obj.Id, obj: response.obj });
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
                            url: `Project/Enable?Id=${id}`,
                            onSuccess: Project.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableProject", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }
    static getFile({ filePath = "", onSuccess }) {
        Data.get({
            url: `Project/GetFile?FilePath=${filePath}`,
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
window.tableProjectSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableProjectStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}</div>`;
}
window.tableProjectCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableProjectPoDate = (value, obj, index) => {
    return `<div>${moment(obj.PoDate).format('DD-MMM-YYYY')}</div>`;
}
window.tableProjectStartDate = (value, obj, index) => {
    return `<div>${moment(obj.StartDate).format('DD-MMM-YYYY')}</div>`;
}
//window.tableProjectStartAt = (value, obj, index) => {
//    return `<div>${moment(obj.StartAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
//}
window.tableProjectDeadLineDate = (value, obj, index) => {
    return `<div>${moment(obj.DeadLineDate).format('DD-MMM-YYYY')}</div>`;
}
//window.tableProjectDeadLineAt = (value, obj, index) => {
//    return `<div>${moment(obj.DeadLineAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
//}
window.tableProjectUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableProjectAction = (value, obj, index) => {
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
window.tableProjectActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Project.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Project.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Project.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Project.enable({ id: obj.Id });
    }
}