class Task {
    static task = [];
    static User = [];
    static currentAssignIndex = null;
   
    static init() {
        Task.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#CustomerDesc', data: response.data.Customer, value: 'Id', text: 'Description' });
           
        });

        $('#btnSearch').on('click', () => {
            Task.get({
                action: "Get",
                onSuccess: (response) => {
                    Table.add({ id: '#tableTask', data: response.data })
                }
            });
        });


        $('#btnPrint').on('click', () => {
            Task.get({
                action: "Print",
                onSuccess: (response) => {
                    Table.add({ id: '#tableTask', data: response.data, isPrint: true });
                }
            });
        });

        $('#btnExport').on('click', () => {
            Task.get({
                action: "Export",
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Task" });
                }
            });
        });
        $('#btnNewEntry').on('click', () => {
            Task.newEntry();
        });
        $('#btnAssignToOk').on('click', () => {

            if (Task.currentAssignIndex === null) return;

            let selectedUsers =
                $('#tableTaskAssignTo')
                    .bootstrapTable('getData')
                    .filter(x => x.IsAdded)
                    .map(x => ({
                        Id: x.Id,
                        Name: x.UserName ?? ''
                        
                    }));

            let taskItems = $('#tableTaskItem').bootstrapTable('getData');

            taskItems[Task.currentAssignIndex].AssignToList = selectedUsers;

            Table.updateByIndex({
                id: '#tableTaskItem',
                index: Task.currentAssignIndex,
                obj: taskItems[Task.currentAssignIndex]
            });

            // cleanup
            Task.currentAssignIndex = null;

            Modal.close({ id: '#modalTaskAssignTo' });
        });

        Task.initAdd();
    
    
    }
    static mergeAssign(allList, assignedList, key = 'Id') {

        const extractIds = (input) => {
            if (!input) return [];

            // flatten everything
            if (Array.isArray(input)) {
                return input.flatMap(extractIds);
            }

            // parse string JSON
            if (typeof input === 'string') {
                try {
                    return extractIds(JSON.parse(input));
                } catch {
                    return [];
                }
            }

            // object with Id
            if (typeof input === 'object' && input[key] != null) {
                return [String(input[key])];
            }

            return [];
        };

        const ids = extractIds(assignedList);

        return (allList || []).map(x => ({
            ...x,
            IsAdded: ids.includes(String(x[key]))
        }));
    }


    //Task View
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Task/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val(),
            ListPartyId: $('#CustomerDesc').val()
           

        }
        Data.post({ url: `Task/${action}`, data: obj, onSuccess: onSuccess });
    }
    //Task Add, Edit, Update & Delete
    static initAdd() {
        $('#TechDocument').on('change', (e) => {
            _File.attach({
                fileId: `#${e.target.id}`,
                readerOnLoad: (obj) => {
                    $('#ViewTechDocument').attr('href', obj.BlobUrl);
                    $('#TechDocument').attr("data-base64", obj.Base64);
                    $('#TechDocument').attr("data-mimetype", obj.MimeType);
                    console.log(obj);
                }
            });
        });
        $('#Task_btnSave').on('click', async () => {
            if (!Field.isMandatory({ class: ".Task-required" })) {
                return;
            }

            let TaskItem = $('#tableTaskItem').bootstrapTable('getData');

            TaskItem.forEach(x => {
                x.AssignToList = x.AssignToList ?? [];
            });

            let obj = Data.serializeToObject({ formId: "#formTask" });

            obj.TaskItem = TaskItem;
            let doc = await _File.info(document.getElementById('TechDocument'));

            obj.DocName = doc?.Name ?? null;
            obj.DocMimeType = doc?.MimeType ?? null;
            obj.DocBase64 = doc?.Base64 ?? null;


            if (!obj.Id) {
                Task.add(obj);
            } else {
                Task.update(obj);
            }
        });
      
    }
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'Task/GetAddOption', onSuccess: onSuccess });
    }   
    static newEntry() {
        Task.getAddOption({
            onSuccess: (response) => {
                Task.task = response.data.Task;
                Task.User = response.data.User;
                Modal.open({ id: '#modalTask', title: 'Task / Add', action: 'add' });
                Dropdown.bind({ id: '#Task-CustomerDesc', data: response.data.Customer, value: 'Id', text: 'Description', subText: 'SubText' });
                Dropdown.bind({ id: '#Task-Category', data: response.data.Category, value: 'Value', text: 'Description', subText: 'SubText' });

                $('#PoDate, #StartDate, #EndDate').val('');


            }
        });
    }
    static add(obj) {
       
        Data.post({ url: 'Task/Add', data: obj, onSuccess: Task.addOnSuccess });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalTask" });
            Table.add({ id: "#tableTask", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static addTaskItem() {
        let obj = {            
            TaskId: 0,            
            Description: '',
            EstimatedDays: 0,            
            StartDateTime: null,
            EndDateTime: null,
            DocName: null,
            DocBase64: null,
            DocMimeType: null,
            DocBlobUrl: null,
            AssignToList: [],
            Remarks: null
        };
        Table.add({ id: '#tableTaskItem', data: obj, action: 'append', dateTimePicker: true });
        
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `Task/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = Array.isArray(response.obj) ? response.obj[0] : response.obj;
                        let TaskItemList = Array.isArray(response.obj) ? response.obj[0].TaskItem : response.obj.TaskItem;
                        Task.User = response.data.User;

                        if (TaskItemList[0]) {
                            if (typeof TaskItemList[0].AssignToList === "string") {
                                try {
                                    obj.AssignToList = JSON.parse(TaskItemList[0].AssignToList);
                                } catch {
                                    obj.AssignToList = [];
                                }
                            }
                        }

                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Task / Edit (Code: ${obj.Code})` : `Task / Add`;
                        Dropdown.bind({ id: '#Task-CustomerDesc', data: response.data.Customer, value: 'Id', text: 'Description', subText: 'SubText' });
                        Dropdown.bind({ id: '#Task-Category', data: response.data.Category, value: 'Value', text: 'Description', subText: 'SubText' });



                        Modal.open({ id: '#modalTask', title: title, action: action, obj: obj });

                        if (obj.DocBase64 && obj.DocMimeType) {

                            let blobUrl = _File.blobUrl({
                                base64: obj.DocBase64,
                                mimeType: obj.DocMimeType
                            });

                            $('#TechDocumentName').text(obj.DocName ?? '');
                            $('#ViewTechDocument').attr('href', blobUrl);

                            _File.setIframe({
                                iframeId: '#TechDocPreview',
                                mimeType: obj.DocMimeType,
                                src: blobUrl
                            });
                        }

                        TaskItemList.forEach(item => {
                            if (item.Base64 && item.MimeType) {
                                item.TechDocBlobUrl = _File.blobUrl({
                                    base64: item.Base64,
                                    mimeType: item.MimeType
                                });
                            }
                        });

                        Table.add({
                            id: '#tableTaskItem',
                            data: TaskItemList,
                            selectPick: true
                        });

                        
                       

                                        
                        
                        
                        if (action == "Edit") {
                            let TechDoc = obj.file;
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
                url: 'Task/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    Task.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalTask" });
            Table.updateById({ id: "#tableTask", objId: response.obj.Id, obj: response.obj });
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
                            url: `Task/Delete?Id=${id}`,
                            onSuccess: Task.deleteOnSuccess
                        }
                    );
                }
            },
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableTask", objId: response.obj.Id, obj: response.obj });
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
                            url: `Task/Enable?Id=${id}`,
                            onSuccess: Task.enableOnSuccess
                        }
                    );
                }
            },
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableTask", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.ListId, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });

        }
    }
    static getFile({ filePath = "", onSuccess }) {
        Data.get({
            url: `Task/GetFile?FilePath=${filePath}`,
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
window.tableTaskSLNo = (value, obj, index) => {
    return index + 1;
}
window.tableTaskItemSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableTaskStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tablePoDate = (value, obj, index) => {
    return `<div>${moment(obj.PoDate).format('DD-MMM-YYYY')}</div>`;
}
window.tableStartDateTime = (value, obj, index) => {
    return `<div>${moment(obj.StartDateTime).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableEndDateTime = (value, obj, index) => {
    return `<div>${moment(obj.EndDateTime).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableTaskDeadLineDate = (value, obj, index) => {
    return `<div>${moment(obj.DeadLineDate).format('DD-MMM-YYYY')}</div>`;
}
window.tableTaskCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableTaskUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableTaskAction = (value, obj, index) => {
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
window.tableTaskActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Task.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Task.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Task.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Task.enable({ id: obj.Id });
    }
}

//Table Task Item
window.tableTaskItemDesc = (value, obj, index) => {
    return `<input type="text" id="TaskItemDesc_${index}" class="form-control form-control-sm mb-0 task-item-desc" value="${obj.Description}" maxlength="200" />
    <input type="text"
               id="TaskItemRemarks_${index}"
               class="form-control form-control-sm mb-0 mt-1 task-item-remarks"
               value="${obj.Remarks ?? ''}"
               maxlength="100"
               placeholder="Remarks" />
    `; 
}
window.tableTaskItemEstimetedDays = (value, obj, index) => {
    return `<input type="text" id="TaskItemEstimetedDays_${index}" class="form-control form-control-sm mb-0 task-item-estimeted-days" value="${obj.EstimatedDays}" oninput="this.value = _Number.validate({value: this.value, dp: 0, min: 0, max: 999})"/>`;
}
window.tableTaskItemStartDateTime = (value, obj, index) => {
    return `<input type="datetime-local" id="TaskItemStartDateTime_${index}" class="form-control form-control-sm mb-0 task-item-start-date-time" value="${obj.StartDateTime??""}"/>`;
}
window.tableTaskItemEndDateTime = (value, obj, index) => {
    return `<input type="datetime-local" id="TaskItemEndDateTime_${index}" class="form-control form-control-sm mb-0 task-item-end-date-time" value="${obj.EndDateTime??""}"/>`;
}
window.tableTaskItemTechnicalDoc = (value, obj, index) => {
    obj.DocBlobUrl = _File.blobUrl({ base64: obj.DocBase64, mimeType: obj.DocMimeType });    
    return `
    <div class="text-nowrap border border-primary rounded">
        <label for="TaskItemTechnicalDoc_${index}" class="btn btn-sm btn-outline-primary">
            <span class="fa fa-paperclip"></span>
        </label>
        <input type="file" id="TaskItemTechnicalDoc_${index}" class="form-control form-control-sm mb-0 task-item-tech-doc hide"/>
        &nbsp;&nbsp;${(obj.DocBlobUrl ? `<a href="${(obj.DocBlobUrl)}" class="text-white" target="_blank">${obj.DocName ?? ""}</a>`:"")}
    </div>
    `;
}
window.tableTaskItemAssignTo = (value, obj, index) => {
    return `
        <div class="text-nowrap  text-center">
            <button type="button"
                    class="btn btn-sm btn-outline-primary btn-assign-to"
                    data-index="${index}">
                <span class="fa fa-pencil"></span>
            </button>
        </div>
    `;
};


window.tableTaskItemEvent = {
    'input .task-item-desc': (e, value, obj, index) => {
        obj.Description = e.currentTarget.value;
       
        Table.updateByIndex({ id: '#tableTaskItem', index: index, obj: obj, value: obj.Description, event: e });
    },
    'input .task-item-remarks': (e, value, obj, index) => {
        obj.Remarks = e.currentTarget.value;

        Table.updateByIndex({
            id: '#tableTaskItem',
            index,
            obj,
            value: obj.Remarks,
            event: e
        });
    },
    'input .task-item-estimeted-days': (e, value, obj, index) => {
        obj.EstimatedDays = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableTaskItem', index: index, obj: obj, value: obj.EstimatedDays, event: e });
    },
    'input .task-item-start-date-time': (e, value, obj, index) => {
        obj.StartDateTime = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableTaskItem', index: index, obj: obj });
    },
    'input .task-item-end-date-time': (e, value, obj, index) => {
        obj.EndDateTime = e.currentTarget.value;
        Table.updateByIndex({ id: '#tableTaskItem', index: index, obj: obj });
    },
    'change .task-item-tech-doc': (e, value, obj, index) => {
        _File.attach({
            fileId: `#${e.currentTarget.id}`,
            readerOnLoad: (file) => {
                obj.DocName = file.Name;
                obj.DocBase64 = file.Base64;
                obj.DocMimeType = file.MimeType;
                obj.DocBlobUrl = file.BlobUrl;
                Table.updateByIndex({ id: '#tableTaskItem', index: index, obj: obj, value: obj.TechDocName });
            }
        });
        
        
    },
    'click .btn-assign-to': (e, value, obj, index) => {

        Task.currentAssignIndex = index;

        // normalize
        obj.AssignToList = obj.AssignToList
            ? (Array.isArray(obj.AssignToList) ? obj.AssignToList : [obj.AssignToList])
            : [];

        // open modal
        Modal.open({
            id: '#modalTaskAssignTo',
            title: 'Assign Task To'
        });
        // load table with merged data
        Table.add({
            id: '#tableTaskAssignTo',
            data: Task.mergeAssign(
                Task.User,
                obj.AssignToList,
                'Id'
            )
        });

    }


}

window.tableTaskAssignTo = (value, obj, index) => {
    return `
        <div class="form-switch" style="min-height:auto!important;">
            <input class="form-check-input is-location-checked"
                   type="checkbox"
                   ${obj.IsAdded ? 'checked' : ''}/>
        </div>
    `;
};


window.tableTaskAssignToEvent = {
    'change .is-location-checked': (e, value, obj, index) => {

       
        obj.IsAdded = $(e.target).is(':checked');

        Table.updateByIndex({
            id: '#tableTaskAssignTo',
            index,
            obj,
            toggle: true
        });

        
        if (Task.currentAssignIndex === undefined) return;

        let taskItems = $('#tableTaskItem').bootstrapTable('getData');

        taskItems[Task.currentAssignIndex].AssignToList =
            $('#tableTaskAssignTo')
                .bootstrapTable('getData')
                .filter(x => x.IsAdded)
                .map(x => ({
                    Id: x.Id,
                    Name: x.UserName ?? x.Description ?? '',
                    Location: x.LocationDesc ?? ''
                }));

        Table.updateByIndex({
            id: '#tableTaskItem',
            index: Task.currentAssignIndex,
            obj: taskItems[Task.currentAssignIndex]
        });
    }
};


window.Task = Task;

