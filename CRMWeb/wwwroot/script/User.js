class User {
    static init() {        
        User.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: ['Description'], isSelectPicker: true });
            Dropdown.bind({ id: '#ListUserType', data: response.data.UserType, value: 'Value', text: ['Description'], isSelectPicker: true });
            Dropdown.bind({ id: '#ListId', data: response.data.User, value: 'Id', text: ['UserId'], subText:'Name', isSelectPicker: true });        
        });
        $('#btnSearch').on('click', () => {
            User.get('Get', (response) => {
                Table.add({ id: '#tableUser', data: response.data });
            });
        });
        $('#btnPrint').on('click', () => {
            User.get('Print', (response) => {
                Table.add({ id: '#tableUser', data: response.data, isPrint: true, reportDesc: "User List" });
            });
        });
        $('#btnExport').on('click', () => {
            User.get('Export', (response) => {
                Export.Base64ToExcel({ base64: response.base64, fielName: "User List" });
            });
        });
        $('#btnAdd').on('click', () => {
            User.fill();             
        });
        $('#ContactNo').on('focusout', function () {
            if (parseFloat($('#ContactNo').val()) < 1000000000 || parseFloat($('#ContactNo').val()) > 9999999999) {
                Message.error({ statusText: "Contact No. should be 10 digit." });
                $('#ContactNo').val('');
                return;
            }
        });        
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".required" })) {
                return;
            }
            var obj = Data.serializeToObject({ formId: "#formUser" });
            obj.DateOfBirth = DateTime.json(obj.DateOfBirth);            
            obj.ApiPermission = $('#tableApi').bootstrapTable('getData').filter(ap => ap.View || ap.Add || ap.Update || ap.Delete || ap.Enable || ap.Print || ap.Import || ap.Export);
            obj.ApRoPermission = $('#tableApprovalRole').bootstrapTable('getData').filter(cp => cp.IsAdded == true);
            if (Field.isNullOrEmpty(obj.Id)) {            
                User.add(obj);
            }
            else {
                User.update(obj);
            }
        });        
    }
    static getViewOption(onSuccess) {
        Data.get({ url: 'User/GetViewOption', onSuccess: onSuccess });          
    }
    static get(action, onSuccess) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListUserType: $('#ListUserType').val(),
            ListId: $('#ListId').val(),
        };
        Data.post({ url: `User/${action}`, data: obj, onSuccess: onSuccess });
    }

    static getAddOption({ id = 0, onSuccess }) {        
        Data.get({ url: `User/GetAddOption?Id=${id}`, onSuccess: onSuccess });
    }
    static fill() {
        User.getAddOption({
            onSuccess: (response) => {
                Modal.open({ id: '#modalUser', title: 'User / Add', action: 'Add' });
                $('#DateOfBirth').val('');
                Dropdown.bind({ id: '#UserType', data: response.data.UserType, value: 'Value', text: ['Description'], isSelectPicker: true });
                Table.add({ id: '#tableApi', data: response.data.Api });
                Table.add({ id: '#tableApprovalRole', data: response.data.AprRole });
            }
        });
    }
    static isAllColChecked(obj, isAllColChecked = false) {
        if (isAllColChecked) {
            if (obj.IsAllColChecked) {
                obj.View = true;
                obj.Add = true;
                obj.Update = true;
                obj.Delete = true;
                obj.Enable = true;
                obj.Print = true;
                obj.Import = true;
                obj.Export = true;
            }
            else {
                obj.IsAllColChecked = false;
                obj.View = false;
                obj.Add = false;
                obj.Update = false;
                obj.Delete = false;
                obj.Enable = false;
                obj.Print = false;
                obj.Import = false;
                obj.Export = false;
            }
        }
        else {
            if (obj.View && obj.Add && obj.Update && obj.Delete && obj.Enable && obj.Print && obj.Import && obj.Export) {
                obj.IsAllColChecked = true;
            }
            else {
                obj.IsAllColChecked = false;
            }
        }
        return obj;
    }
    static add(obj) {
        Data.post({ url: 'User/Add', data: obj, onSuccess: User.addOnSuccess });        
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: '#modalUser' });
            Table.add({ id: "#tableUser", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.User, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static reSentPassword(id) {
        Data.get({ url: `User/ReSentPassword?Id=${id}`, onSuccess: (response) => { Message.show(response) } });
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `User/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {                        
                        Dropdown.bind({ id: '#UserType', data: response.data.UserType, value: 'Value', text: ['Description'], isSelectPicker: true });
                        Dropdown.bind({ id: '#DepartmentId', data: response.data.Department, value: 'Id', text: ['Description'], isSelectPicker: true });
                        Dropdown.bind({ id: '#DesignationId', data: response.data.Designation, value: 'Id', text: ['Description'], isSelectPicker: true });
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `User / Edit (Code: ${obj.Code})` : `User / Add`;
                        Modal.open({ id: '#modalUser', title: title, action: action, obj: obj });
                        Table.add({ id: '#tableCompany', data: response.data.Company });
                        Table.add({ id: '#tableApi', data: response.data.Api });
                        Table.add({ id: '#tableApprovalRole', data: response.data.AprRole });
                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update({ url: 'User/Update', data: obj, onSuccess: User.updateOnSuccess });        
    }  
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: '#modalUser' });
            Table.updateById({ id: "#tableUser", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.User, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static delete(id) {
        Message.confirm(
            {
                msg: 'Do you want to delete??',
                confirmButtonText: 'Delete',
                denyButtonText: 'Dont Delete',
                data: id,
                onConfirm: (id) => {
                    Data.delete({ url: `User/Delete?Id=${id}`, onSuccess: User.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableUser", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.User, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static enable(id) {
        Message.confirm(
            {
                msg: 'Do you want to enable??',
                confirmButtonText: 'Enable',
                denyButtonText: 'Dont Enable',
                data: id,
                onConfirm: (id) => {
                    Data.update({ url: `User/Enable?Id=${id}`, onSuccess: User.enableOnSuccess });
                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableUser", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.User, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
}
//===================Table User==========================//
tableUserEmail = (value, obj, index) => {    
    return obj.Email.match(/.{1,20}/g).join('<br>');
}
tableUserPasswordExpiredAt = (value, obj, index) => {
    return DateTime.dateTime(obj.PasswordExpiredAt, true);
}
tableUserStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}<div>`;
}
tableUserCreatedByAndAt = (value, obj, index) => {
    return `<table class="table-child"><tr><td>${obj.CreatedByName}</td></tr><tr><td>${DateTime.dateTime(obj.CreatedAt)}</td></tr></table>`;
}
tableUserUpdatedByAndAt = (value, obj, index) => {
    return `<table class="table-child"><tr><td>${obj.UpdatedByName}</td></tr><tr><td>${DateTime.dateTime(obj.UpdatedAt)}</td></tr></table>`;
}
tableUserAction = (value, obj, index) => {
   let actionBtn = `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsReSentPassword ?
                    `<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-re-sent-password" title="View / Edit">
                            <span class="fa fa-paper-plane text-primary-100"></span>&nbsp;&nbsp;Re-Sent Password
                        </a>
                    </li>` : ``
                }
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
window.tableUserActionEvent = {
    'click .btn-re-sent-password': (e, value, obj, index) => {
        User.reSentPassword(obj.Id);
    },
    'click .btn-edit': (e, value, obj, index) => {
        User.edit({id: obj.Id });
    },    
    'click .btn-duplicate': (e, value, obj, index) => {
        User.edit({ id: obj.Id, action: "Add" });
    },
    'click .btn-delete': (e, value, obj, index) => {
        User.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        User.enable(obj.Id);
    }
}

//===================Table Company Permission==========================//
tableCompanyCheckedFormatter = (value, obj, index) => {
    let checkBox = `
        <div class="form-switch" style="min-height:auto!important;">
            <input class="form-check-input company-checked" type="checkbox" id="CheckBoxCompany_${index}" ${obj.IsAdded ? 'checked' : ''}/>
        </div>
    `;       
    return checkBox;
}
window.tableCompanyCheckedEvent = {
    'change .company-checked': (e, value, obj, index) => {
        obj.IsAdded = $(e.target).is(':Checked') ? true : false;
        $('#tableCompany').bootstrapTable('updateRow', { index: index, row: obj });
    }
}
tableCompanyIsDefaultFormatter = (value, obj, index) => {
    let checkBox = `
        <div class="form-switch" style="min-height:auto!important;">
            <input class="form-check-input default-checked" type="checkbox" id="CheckBoxDefault_${index}" ${obj.IsDefault ? 'checked' :''}/>
        </div>
    `;    
    return checkBox;
}
window.tableCompanyIsDefaultEvent = {
    'change .default-checked': (e, value, obj, index) => {
        obj.IsDefault = $(e.target).is(':Checked') ? true : false;
        $('#tableCompany').bootstrapTable('updateRow', { index: index, row: obj });
        var Company = $('#tableCompany').bootstrapTable('getData').map((com) => {
            if (obj.CompanyId != com.CompanyId) {
                    com.IsDefault = false;
                }
                return com;
            }
        );
        Table.add({ id: '#tableCompany', data: Company });
    }
}
//===================Table Api Permission==========================//
tableApiIsAllColChecked = (value, obj, index) => {
    obj.IsAllColChecked = obj.View && obj.Add && obj.Update && obj.Delete && obj.Enable && obj.Print && obj.Export && obj.Import ? true : false;
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input all-col-checked" type="checkbox" id="CheckBoxAllCol_' + index + '"');
    checkBox.push(obj.IsAllColChecked ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiIsAllColCheckedEvent = {
    'change .all-col-checked': (e, value, obj, index) => {
        obj.IsAllColChecked = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj, true);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });        
    }
}
tableApiView = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input checkbox-view" type="checkbox" id="CheckBoxView_' + index + '"');
    checkBox.push(obj.View ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiViewEvent = {
    'change .checkbox-view': (e, value, obj, index) => {
        obj.View = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });
    }
}
tableApiAdd = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input checkbox-add" type="checkbox" id="CheckBoxAdd_' + index + '"');
    checkBox.push(obj.Add ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiAddEvent = {
    'change .checkbox-add': (e, value, obj, index) => {
        obj.Add = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });
    }
}
tableApiUpdate = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input checkbox-update" type="checkbox" id="CheckBoxUpdate_' + index + '"');
    checkBox.push(obj.Update ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiUpdateEvent = {
    'change .checkbox-update': (e, value, obj, index) => {
        obj.Update = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });
    }
}
tableApiDelete = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input checkbox-delete" type="checkbox" id="CheckBoxDelete_' + index + '"');
    checkBox.push(obj.Delete ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiDeleteEvent = {
    'change .checkbox-delete': (e, value, obj, index) => {
        obj.Delete = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });
    }
}
tableApiEnable = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input checkbox-enable" type="checkbox" id="CheckBoxEnable_' + index + '"');
    checkBox.push(obj.Enable ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiEnableEvent = {
    'change .checkbox-enable': (e, value, obj, index) => {
        obj.Enable = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });
    }
}
tableApiPrint = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input checkbox-print" type="checkbox" id="CheckBoxPrint_' + index + '"');
    checkBox.push(obj.Print ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiPrintEvent = {
    'change .checkbox-print': (e, value, obj, index) => {
        obj.Print = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });
    }
}
tableApiImport = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input checkbox-import" type="checkbox" id="CheckBoxImport_' + index + '"');
    checkBox.push(obj.Import ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiImportEvent = {
    'change .checkbox-import': (e, value, obj, index) => {
        obj.Import = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });
    }
}
tableApiExport = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input checkbox-export" type="checkbox" id="CheckBoxExport_' + index + '"');
    checkBox.push(obj.Export ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApiExportEvent = {
    'change .checkbox-export': (e, value, obj, index) => {
        obj.Export = $(e.target).is(':Checked') ? true : false;
        obj = User.isAllColChecked(obj);
        Table.updateById({ id: '#tableApi', objId: obj.ApiId, obj: obj });
    }
}

//===================Table Approval Role Permission==========================//
tableApprovalRoleChecked = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input user-approval-role" type="checkbox" id="CheckBoxApprovalRole_' + index + '"');
    checkBox.push(obj.IsAdded ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableApprovalRoleCheckedEvent = {
    'change .user-approval-role': (e, value, obj, index) => {
        obj.IsAdded = $(e.target).is(':Checked') ? true : false;
        Table.updateById({ id:'#tableApprovalRole', objId: obj.ApprovalRoleId, obj: obj});             
    }
}
