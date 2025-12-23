class Qualification {

    static init() {
        Qualification.getViewOption((response) => {
            Dropdown.bind({
                id: '#ListStatus',
                data: response.data.Status,
                value: 'Value',
                text: 'Description'
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Qualification,
                value: 'Id',
                text: 'Description',
                subText: 'Code'
            });
        });

        $('#btnSearch').on('click', () => {
            Qualification.get();
        });

        $('#btnPrint').on('click', () => {
            Qualification.print();
        });

        $('#btnExport').on('click', () => {
            Qualification.export();
        });

        $('#btnAdd').on('click', () => {
            Qualification.fill();
        });

        Qualification.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({
            url: 'Qualification/GetViewOption',
            onSuccess: onSuccess
        });
    }

    static get() {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        };

        Data.post({
            url: 'Qualification/Get',
            data: obj,
            onSuccess: (response) => {
                Table.add({
                    id: '#tableQualification',
                    data: response.data
                });
            }
        });
    }

    static print() {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        };

        Data.post({
            url: 'Qualification/Print',
            data: obj,
            onSuccess: (response) => {
                Table.add({
                    id: '#tableQualification',
                    data: response.data,
                    isPrint: true
                });
            }
        });
    }

    static export() {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        };

        Data.post({
            url: 'Qualification/Export',
            data: obj,
            onSuccess: (response) => {
                Export.Base64ToExcel({
                    base64: response.base64,
                    fielName: 'Qualification'
                });
            }
        });
    }

    static initAdd() {
        $('#Qualification_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.qualification-required' })) {
                return;
            }

            let obj = Data.serializeToObject({
                formId: '#formQualification'
            });

            if (!obj.Id) {
                Qualification.add(obj);
            } else {
                Qualification.update(obj);
            }
        });
    }

    static getAddOption(onSuccess) {
        Data.get({
            url: 'Qualification/GetAddOption',
            onSuccess: onSuccess
        });
    }

    static fill() {
        Qualification.getAddOption(() => {
            Modal.open({
                id: '#modalQualification',
                title: 'Qualification / Add',
                action: 'Add'
            });
        });
    }

    static add(obj) {
        Data.post({
            url: 'Qualification/Add',
            data: obj,
            onSuccess: Qualification.addOnSuccess
        });
    }

    static addOnSuccess = (response) => {
        Message.show(response);

        if (response.status === Message.Type.success) {
            Modal.reset({ id: '#modalQualification' });

            Table.add({
                id: '#tableQualification',
                data: response.obj,
                action: 'prepend'
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Qualification,
                value: 'Id',
                text: 'Description',
                subText: 'Code'
            });
        }
    };

    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `Qualification/Edit?Id=${id}`,
            onSuccess: (response) => {
                if (response.status === Message.Type.success) {

                    let obj = response.obj;
                    obj.Id = action === 'Edit' ? obj.Id : null;

                    let title = action === 'Edit'
                        ? `Qualification / Edit (Code: ${obj.Code})`
                        : 'Qualification / Add';

                    Modal.open({
                        id: '#modalQualification',
                        title: title,
                        action: action,
                        obj: obj
                    });
                }
                else {
                    Message.show(response);
                }
            }
        });
    }

    static update(obj) {
        Data.update({
            url: 'Qualification/Update',
            data: obj,
            onSuccess: Qualification.updateOnSuccess
        });
    }

    static updateOnSuccess = (response) => {
        Message.show(response);

        if (response.status === Message.Type.success) {
            Modal.close({ id: '#modalQualification' });

            Table.updateById({
                id: '#tableQualification',
                objId: response.obj.Id,
                obj: response.obj
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Qualification,
                value: 'Id',
                text: 'Description',
                subText: 'Code'
            });
        }
    };

    static delete({ id }) {
        Message.confirm({
            msg: 'Do you want to delete?',
            confirmButtonText: 'Delete',
            denyButtonText: 'Don’t Delete',
            data: id,
            onConfirm: (id) => {
                Data.delete({
                    url: `Qualification/Delete?Id=${id}`,
                    onSuccess: Qualification.deleteOnSuccess
                });
            }
        });
    }

    static deleteOnSuccess = (response) => {
        Message.show(response);

        if (response.status === Message.Type.success) {
            Table.updateById({
                id: '#tableQualification',
                objId: response.obj.Id,
                obj: response.obj
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Qualification,
                value: 'Id',
                text: 'Description',
                subText: 'Code'
            });
        }
    };

    static enable({ id }) {
        Message.confirm({
            msg: 'Do you want to enable?',
            confirmButtonText: 'Enable',
            denyButtonText: 'Don’t Enable',
            data: id,
            onConfirm: (id) => {
                Data.update({
                    url: `Qualification/Enable?Id=${id}`,
                    onSuccess: Qualification.enableOnSuccess
                });
            }
        });
    }

    static enableOnSuccess = (response) => {
        Message.show(response);

        if (response.status === Message.Type.success) {
            Table.updateById({
                id: '#tableQualification',
                objId: response.obj.Id,
                obj: response.obj
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Qualification,
                value: 'Id',
                text: 'Description',
                subText: 'Code'
            });
        }
    };
}

window.tableQualificationSLNo = (value, obj, index) => {
    return index + 1;
};

window.tableQualificationStatus = (value, obj) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}</div>`;
};

window.tableQualificationCreatedByAndAt = (value, obj) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>
    `;
};

window.tableQualificationUpdatedByAndAt = (value, obj) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>
    `;
};

window.tableQualificationAction = (value, obj) => {
    return `
        <div class="btn-group dropstart">
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ? `
                <li>
                    <a href="#" class="dropdown-item text-success-100 btn-edit">
                        <span class="fa fa-edit"></span>&nbsp;&nbsp;View / Edit
                    </a>
                </li>` : ``}

                ${obj.IsDuplicate ? `
                <li>
                    <a href="#" class="dropdown-item text-primary-100 btn-duplicate">
                        <span class="fa fa-copy"></span>&nbsp;&nbsp;Duplicate
                    </a>
                </li>` : ``}

                ${obj.IsDelete ? `
                <li>
                    <a href="#" class="dropdown-item text-danger-100 btn-delete">
                        <span class="fa fa-trash"></span>&nbsp;&nbsp;Delete
                    </a>
                </li>` : ``}

                ${obj.IsEnable ? `
                <li>
                    <a href="#" class="dropdown-item text-success-100 btn-enable">
                        <span class="fa fa-toggle-on"></span>&nbsp;&nbsp;Enable
                    </a>
                </li>` : ``}
            </ul>
        </div>
    `;
};

window.tableQualificationActionEvent = {
    'click .btn-edit': (e, value, obj) => {
        Qualification.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj) => {
        Qualification.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj) => {
        Qualification.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj) => {
        Qualification.enable({ id: obj.Id });
    }
};
