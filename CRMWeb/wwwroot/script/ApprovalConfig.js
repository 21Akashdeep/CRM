let dragElementId;
class ApprovalConfig {
    static init() {
        ApprovalConfig.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description', subText: "Code" });
                Dropdown.bind({ id: '#ListApiId', data: response.data.Api, value: 'Id', text: 'Description', subText: "Code" });
            }
        });
        $('#btnSearch').on('click', () => {
            ApprovalConfig.get({
                onSuccess: (response) => {
                    Table.add({ id: '#tableApprovalConfig', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            ApprovalConfig.get({
                ApiName: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableApprovalConfig', data: response.data, isPrint: true, reportDesc: "Approval Config" });
                }
            });
        });
        $('#btnExport').on('click', () => {
            ApprovalConfig.get({
                ApiName: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fielName: "Approval Config" });
                }
            });            
        });
        $('#btnAdd').on('click', () => {
            ApprovalConfig.fill();        
        });
        ApprovalConfig.initAdd();
    }
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'ApprovalConfig/GetViewOption', onSuccess: onSuccess });
    }
    static get({ ApiName = "Get", onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListModuleName:$('#ListModaleName').val()
        };
        Data.post({ url: `ApprovalConfig/${ApiName}`, data: obj, onSuccess: onSuccess });
    }
    

    static initAdd() {
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) return;
            let ApprovalSeq = [...$("#ApprovalConfig .approval-role-item")];
            if (ApprovalSeq.length == 0) {
                Message.show({ statusText: 'Approval Config is empty.' });
                return;
            }
            let obj = {
                ApiId: $('#ApiId').val(),
                ApiDesc: $('#ApiId option:selected').text(),
                HtmlString: $('#ApprovalConfig').html().replace(/\s+/g, ' ').replace('<div class="watermark-text"></div>', '').trim(),
                ApprovalSeq: ApprovalSeq.map(item => ({
                    ApprovalRoleId: item.getAttribute("data-approvalroleid"),
                    SeqNo: item.getAttribute("data-seqno"),
                }))
            };            
            ApprovalConfig.add(obj);
        });
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'ApprovalConfig/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        ApprovalConfig.getAddOption({
            onSuccess: (response) => {
                Modal.open({ id: '#modalApprovalConfig', title: 'Approval Config / Add', action: 'Add' });                                
                let ApprovalRole = (response.data.ApprovalRole ?? []).map(objItem => `
                    <div id="AR_${objItem.Name}" class="approval-role-item" data-ApprovalRoleId="${objItem.Id}" data-SeqNo="1" draggable="true" ondragstart="ApprovalConfig.drag(event)">
                        <span class="fa fa-times remove-approval-role" data-index="${objItem.Id}"></span>&nbsp;&nbsp;
                        <span class="item-text">${objItem.Description}</span>
                        <span class="item-seq-no"></span>
                    </div>
                `).join('');
                $('#ApprovalRole').html(ApprovalRole);
                $('#ApprovalConfig').html('');
                Dropdown.bind({ id: '#ApiId', data: response.data.Api, value: 'Id', text: 'Description', subText: "Code" });
            }
        });        
    }    
    static drag(ev) {
        ev.dataTransfer.setData("text", ev.target.id);
        dragElementId = ev.target.id;
    }
    static allowDrop(ev) {
        ev.preventDefault();
    }
    static drop(ev) {
        ev.preventDefault();
        let parentId = ev.target.id;        
        if (!Field.isNullOrEmpty(parentId)) {
            let data = ev.dataTransfer.getData("text");
            ev.target.appendChild(document.getElementById(data));
            let seqNo = parentId == 'ApprovalConfig' ? 1 : (parseInt($(`#${parentId}`).attr('data-seqno')) + 1);            
            $(`#${dragElementId}`).attr('data-SeqNo', seqNo);
            let ItemSeqNoText = `(Level : ${seqNo})`;
            $(`#${dragElementId} span.item-seq-no`).text(ItemSeqNoText);
            $('#ApprovalConfig .watermark-text').text('');
            ApprovalConfig.removeApprovalRoleItem();
        }        
    }
    static removeApprovalRoleItem() {
        $('#ApprovalConfig .remove-approval-role').off('click');
        $('#ApprovalConfig .remove-approval-role').on('click', function () {
            let parent = $(this).parent();
            if (parent.find('div').length > 0) {
                Message.error({ statusText: 'First remove child then remove parent' });
                return;
            }
            $(`#${parent.attr('id')}`).attr('data-seqno', '1');
            $(`#${parent.attr('id')} span.item-seq-no`).text('');
            $(`#${parent.attr('id')}`).clone().appendTo("#ApprovalRole");
            $(`#ApprovalConfig #${parent.attr('id')}`).remove();
            if ($('#ApprovalConfig .approval-role-item').length == 0) {
                $('#ApprovalConfig .watermark-text').text('Drop Here');
            }            
        });
    }
    static add(List) {
        Data.post({ url: 'ApprovalConfig/Add', data: List, onSuccess: ApprovalConfig.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: '#modalApprovalConfig' });
            $('#ApprovalConfig .remove-approval-role').get().reverse().forEach(function (item) {               
                $(item).trigger('click'); 
            });                     
            Table.add({ id: "#tableApprovalConfig", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description', subText: "Code" });
        }
    }
    static delete(Id) {
        Message.confirm(
            {
                msg: 'Do you want to delete??',
                confirmButtonText: "Delete",
                denyButtonText: "Dont Delete",
                data: Id,
                onConfirm: (Id) => {
                    Data.delete({ url: `ApprovalConfig/Delete?Id=${Id}`, onSuccess: ApprovalConfig.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableApprovalConfig", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description', subText: "Code" });
        }
    }    
    static enable(Id) {
        Message.confirm(
            {
                msg: 'Do you want to enable??',
                confirmButtonText: "Enable",
                denyButtonText: "Dont Enable",
                data: Id,
                onConfirm: (Id) => {
                    Data.update({ url: `ApprovalConfig/Enable?Id=${Id}`, onSuccess: ApprovalConfig.deleteOnSuccess });
                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableApprovalConfig", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description', subText: "Code" });
        }
    }
}
window.tableApprovalConfigSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableApprovalHtmlString = (value, obj, index) => {    
    let ApprovalSeq = document.createElement('div');
    ApprovalSeq.innerHTML = obj.HtmlString;    
    ApprovalSeq.querySelectorAll('.approval-role-item').forEach(element => {
        element.removeAttribute('id');
        element.removeAttribute('draggable');
        element.removeAttribute('ondragstart');
    });
    return ApprovalSeq.innerHTML;
}
window.tableApprovalConfigCreatedByAndAt = (value, obj, index) => {
    return obj.CreatedByName + "<br>" + moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
window.tableApprovalConfigUpdatedByAndAt = (value,obj, index) => {
    return obj.UpdatedByName + "<br>" + moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss');
}
window.tableApprovalConfigStatus = (value, obj, index) => {
    return '<div class="' + obj.StatusCss + '">' + obj.StatusDesc + '<div>';
}
window.tableApprovalConfigAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">                
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

window.tableApprovalConfigActionEvent = {
    'click .btn-delete': (e, value, obj, index) => {
        ApprovalConfig.delete(obj.Id);
    },
    'click .btn-enable': (e, value, obj, index) => {
        ApprovalConfig.enable(obj.Id);
    }
}