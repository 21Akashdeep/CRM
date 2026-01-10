class Store {

    static init() {
        Store.getViewOption((response) => {
            Dropdown.bind({
                id: '#ListStatus',
                data: response.data.Status,
                value: 'Value',
                text: 'Description'
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Store,
                value: 'Id',
                text: 'Description',
                subText: 'Code'
            });
        });

        $('#btnSearch').on('click', () => {
            Store.get();
        });

        $('#btnPrint').on('click', () => {
            Store.print();
        });

        $('#btnExport').on('click', () => {
            Store.export();
        });

        $('#btnAdd').on('click', () => {
            Store.fill();
        });

        Store.initAdd();
    }

    static getViewOption(onSuccess = () => { }) {
        Data.get({
            url: 'Store/GetViewOption',
            onSuccess: onSuccess
        });
    }

    static get() {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        };

        Data.post({
            url: 'Store/Get',
            data: obj,
            onSuccess: (response) => {
                Table.add({
                    id: '#tableStore',
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
            url: 'Store/Print',
            data: obj,
            onSuccess: (response) => {
                Table.add({
                    id: '#tableStore',
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
            url: 'Store/Export',
            data: obj,
            onSuccess: (response) => {
                Export.Base64ToExcel({
                    base64: response.base64,
                    fielName: 'Store'
                });
            }
        });
    }

    static initAdd() {
        $('#Store_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.store-required' })) {
                return;
            }

            let obj = Data.serializeToObject({
                formId: '#formStore'
            });

            if (!obj.Id) {
                Store.add(obj);
            } else {
                Store.update(obj);
            }
        });
    }

    static getAddOption(onSuccess) {
        Data.get({
            url: 'Store/GetAddOption',
            onSuccess: onSuccess
        });
    }

    static fill() {
        Store.getAddOption(() => {
            Modal.open({
                id: '#modalStore',
                title: 'Store / Add',
                action: 'Add'
            });
        });
    }

    static add(obj) {
        Data.post({
            url: 'Store/Add',
            data: obj,
            onSuccess: Store.addOnSuccess
        });
    }

    static addOnSuccess = (response) => {
        Message.show(response);

        if (response.status === Message.Type.success) {
            Modal.reset({ id: '#modalStore' });

            Table.add({
                id: '#tableStore',
                data: response.obj,
                action: 'prepend'
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Store,
                value: 'Id',
                text: 'Description',
                subText: 'Code'
            });
        }
    };

    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `Store/Edit?Id=${id}`,
            onSuccess: (response) => {
                if (response.status === Message.Type.success) {

                    let obj = response.obj;
                    obj.Id = action === 'Edit' ? obj.Id : null;

                    let title = action === 'Edit'
                        ? `Store / Edit (Code: ${obj.Code})`
                        : 'Store / Add';

                    Modal.open({
                        id: '#modalStore',
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
            url: 'Store/Update',
            data: obj,
            onSuccess: Store.updateOnSuccess
        });
    }

    static updateOnSuccess = (response) => {
        Message.show(response);

        if (response.status === Message.Type.success) {
            Modal.close({ id: '#modalStore' });

            Table.updateById({
                id: '#tableStore',
                objId: response.obj.Id,
                obj: response.obj
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Store,
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
                    url: `Store/Delete?Id=${id}`,
                    onSuccess: Store.deleteOnSuccess
                });
            }
        });
    }

    static deleteOnSuccess = (response) => {
        Message.show(response);

        if (response.status === Message.Type.success) {
            Table.updateById({
                id: '#tableStore',
                objId: response.obj.Id,
                obj: response.obj
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Store,
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
                    url: `Store/Enable?Id=${id}`,
                    onSuccess: Store.enableOnSuccess
                });
            }
        });
    }

    static enableOnSuccess = (response) => {
        Message.show(response);

        if (response.status === Message.Type.success) {
            Table.updateById({
                id: '#tableStore',
                objId: response.obj.Id,
                obj: response.obj
            });

            Dropdown.bind({
                id: '#ListId',
                data: response.data.Store,
                value: 'Id',
                text: 'Description',
                subText: 'Code'
            });
        }
    };
}

/* ---------- Table Formatters ---------- */

window.tableStoreSLNo = (value, obj, index) => {
    return index + 1;
};

window.tableStoreStatus = (value, obj) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}</div>`;
};

window.tableStoreCreatedByAndAt = (value, obj) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>
    `;
};

window.tableStoreUpdatedByAndAt = (value, obj) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>
    `;
};

window.tableStoreAction = (value, obj) => {
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

window.tableStoreActionEvent = {
    'click .btn-edit': (e, value, obj) => {
        Store.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj) => {
        Store.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj) => {
        Store.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj) => {
        Store.enable({ id: obj.Id });
    }
};
