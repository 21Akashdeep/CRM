class ApprovalRole {
    static init() {
        ApprovalRole.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ApprovalRole, value: 'Id', text: 'Description' });
        });
        $('#btnSearch').on('click', () => {
            ApprovalRole.get();
        });
        $('#btnPrint').on('click', () => {
            ApprovalRole.print();
        });
        $('#btnExport').on('click', () => {
            ApprovalRole.export();
        });
        $('#btnAdd').on('click', () => {
            ApprovalRole.fill();
        });
        ApprovalRole.initAdd();
    }
    static getViewOption(onSuccess) {
        Data.get({ url: 'ApprovalRole/GetViewOption', onSuccess: onSuccess });
    }
    static get() {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        };
        Data.post(
            {
                url: 'ApprovalRole/Get',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableApprovalRole', data: response.data });  
                }
            }
        );
    }
    static print() {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        };
        Data.post(
            {
                url: 'ApprovalRole/Print',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableApprovalRole', data: response.data, isPrint: true, reportDesc: "Approval Role" });
                }
            }
        );
    }
    static export() {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        };
        Data.post(
            {
                url: 'ApprovalRole/Export',
                data: obj,
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Approval Role" });
                }
            }
        );
    }

    static initAdd() {
        $('#ApprovalRole_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".approval-role" })) {
                return;
            }
            var obj = Data.serializeToObject({ formId: "#formApprovalRole" });
            if (Field.isNullOrEmpty(obj.Id)) {
                ApprovalRole.add(obj);
            }
            else {
                ApprovalRole.update(obj);
            }
        });
    }
    static fill() {
        Modal.open({ id: '#modalApprovalRole', title: 'Approval Role / Add', action: 'Add' });
    }
    static add(obj) {
        Data.post({ url: 'ApprovalRole/Add', data: obj, onSuccess: ApprovalRole.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: '#modalApprovalRole' });
            Table.add({ id: "#tableApprovalRole", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.ApprovalRole, value: 'Id', text: ['Description'] });
        }
    }
    static edit(id, viewMode = 'Edit') {
        Data.get(
            {
                url: `ApprovalRole/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = viewMode == 'Edit' ? obj.Id : null;
                        let title = viewMode == 'Edit' ? `Approval Role / Edit (Code : ${obj.Code})` : 'Approval Role / Add';                        
                        Modal.open({ id: '#modalApprovalRole', title: title, action: viewMode, obj: obj });                        
                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update({ url: 'ApprovalRole/Update', data: obj, onSuccess: ApprovalRole.updateOnSuccess });        
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: '#modalApprovalRole' });
            Table.updateById({ id: "#tableApprovalRole", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ApprovalRole, value: 'Id', text: ['Description'] });
        }
    }
    static delete(id) {
        Message.confirm(
        {
            msg: "Don you want to delete.",
            confirmButtonText: "Delete",
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (Id) => {
                Data.delete(
                    {
                        url: `ApprovalRole/Delete?Id=${Id}`,
                        onSuccess: (response) => {
                            ApprovalRole.deleteOnSuccess(response);
                        }
                    }
                );
            }
        });
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableApprovalRole", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ApprovalRole, value: 'Id', text: ['Description'] });            
        }
    };
    static enable(id) {
        Message.confirm(
            {
                msg: "Don you want to enable.",
                confirmButtonText: "Enable",
                denyButtonText: "Don't Enable",
                data: id,
                onConfirm: (Id) => {
                    Data.update(
                        {
                            url: `ApprovalRole/Enable?Id=${Id}`,
                            onSuccess: (response) => {
                                ApprovalRole.enableOnSuccess(response);
                            }
                        }
                    );
                }
            });
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableApprovalRole", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ApprovalRole, value: 'Id', text: ['Description'] });            
        }
    };
}

window.tableApprovalRoleStatus = (value, obj, index) => {    
    return `<div class="${obj.StatusCss}">${obj.StatusName}<div>`;
}
window.tableApprovalRoleCreatedByAndAt = (value, obj, index) => {
    return `${obj.CreatedByName} <br> ${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}`;
}
window.tableApprovalRoleUpdatedByAndAt = (value, obj, index) => {
    return `${obj.UpdatedByName} <br> ${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}`;
}
window.tableApprovalRoleAction = (value, obj, index) => {
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
window.tableApprovalRoleActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        ApprovalRole.edit(obj.Id);
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        ApprovalRole.edit(obj.Id, true);
    },
    'click .btn-delete': (e, value, obj, index) => {
        ApprovalRole.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        ApprovalRole.enable(obj.Id);
    },
}
