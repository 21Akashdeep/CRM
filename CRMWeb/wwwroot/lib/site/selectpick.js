//Global Data
const
    SELECTPICK = "select-pick",
    SELECTPICK_DATA_ID = "data-Id",
    SELECTPICK_DATA_SIZE = "data-size",
    SELECTPICK_DATA_PARENT = "data-parent",
    SELECTPICK_DATA_SEARCH_BOX = "data-search",
    SELECTPICK_DATA_ACTION_BOX = "data-action",
    SELECTPICK_DATA_ADDFN = "data-addFn",
    SELECTPICK_DATA_EDITFN = "data-editFn",
    SELECTPICK_DATA_DELETEFN = "data-deleteFn",
    SELECTPICK_OPTION_MULTIPLE = "multiple",
    SELECTPICK_TOGGLE_BTN = 'select-pick-toggle-btn',
    SELECTPICK_TOGGLE_BTN_DATA_TARGET = 'data-target',
    SELECTPICK_TOGGLE_BTN_TEXT = "toggle-text",
    SELECTPICK_TOGGLE_BTN_ICON = "toggle-icon",
    SELECTPICK_MENU = "select-pick-menu",
    SELECTPICK_MENU_SHOW = "menu-show",
    SELECTPICK_MENU_SEACRH_BOX = "menu-search-box",
    SELECTPICK_MENU_ACTION_BOX = "menu-action-box",
    SELECTPICK_MENU_ITEM_ADD = "menu-item-add",
    SELECTPICK_MENU_ACTION_ADDNEW = "btn-add-new",
    SELECTPICK_MENU_ACTION_SELECTALL = "btn-select-all",
    SELECTPICK_MENU_ACTION_DESELECTALL = "btn-deselect-all",
    SELECTPICK_MENU_LIST = "menu-list",
    SELECTPICK_MENU_ITEM = "menu-item",
    SELECTPICK_ITEM_ICON = "item-icon",
    SELECTPICK_ITEM_TEXT = "item-text",
    SELECTPICK_ITEM_SUBTEXT = "item-sub-text",
    SELECTPICK_ITEM_BTN = "item-btn-group",
    SELECTPICK_ITEM_SELECTED = "item-selected",
    SELECTPICK_ITEM_HTML = "item-html",    
    SELECTPICK_ITEM_DATA_ICON = "data-icon",
    SELECTPICK_ITEM_DATA_SUBTEXT = "data-subtext",
    SELECTPICK_ITEM_DATA_HTML = "data-html";
class SelectPick {
    static #data = [];
    static #setData(select) {
        if (select.id == undefined || select.id == '') {
            console.error(`Select id is undefiend or empty`);
            return;
        }
        let obj = {};
        obj.id = select.id;
        obj.title = select.title == '' ? 'Nothing Selected' : select.title;
        obj.menuId = `${select.id}-SelectPick-Menu`;        
        obj.searchBox = SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_SEARCH_BOX, intialVal: 'false' });
        obj.isSearched = false;
        obj.actionBox = SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_ACTION_BOX, intialVal: 'false' });
        obj.isMultiple = select.attributes[SELECTPICK_OPTION_MULTIPLE] == undefined ? false : true;
        obj.addFn = SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_ADDFN, isfn: true });
        obj.parent = SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_PARENT, intialVal: 'body' });        
        obj.size = parseInt(SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_SIZE, intialVal: '0' }));                
        obj.isDisabled = select.disabled;
        obj.data = [];
        let options = select.options;
        for (let index = 0; index < options.length; index++) {
            const op = options[index];
            let option = {};
            option.id = `${obj.id}-select-pick-item-${index}`;
            option.value = op.value;
            option.text = op.text;
            option.isSelected = op.selected;
            option.subText = SelectPick.#getAttrVal({ el: op, attr: SELECTPICK_ITEM_DATA_SUBTEXT });
            option.icon = SelectPick.#getAttrVal({ el: op, attr: SELECTPICK_ITEM_DATA_ICON });
            option.html = SelectPick.#getAttrVal({ el: op, attr: SELECTPICK_ITEM_DATA_HTML });
            option.editFn = SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_EDITFN, isfn: true, fnParm: option.value });
            option.deleteFn = SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_DELETEFN, isfn: true, fnParm: option.value });
            option.obj = op;
            obj.data.push(option);
        }
        //Remove Duplicate Data
        delete SelectPick.#data[SelectPick.#data.findIndex(x => x.id == obj.id)];
        SelectPick.#data = SelectPick.#data.filter(x => Object.keys(x).length != 0);
        //Push Unique Data
        SelectPick.#data.push(obj);
    }
    static #getAttrVal({ el = {}, attr = '', initVal = null, isfn= false, fnParm= null }) {
        if (el.attributes[attr] == undefined) {
            return initVal;
        }
        else if (el.attributes[attr].value == "") {
            return initVal;
        }
        else if (isfn) {
            if (fnParm != null) {
                return el.attributes[attr].value == '' ? '' : `${el.attributes[attr].value}({id:${fnParm}})`;
            }
            else {
                return el.attributes[attr].value == '' ? '' : `${el.attributes[attr].value}()`;
            }
        }
        else {
            return el.attributes[attr].value
        }
    }
    static render(select) {  
        document.querySelector(`#${select.id}`).classList.add('select-pick');
        //Set Data
        SelectPick.#setData(select);
        //Remove Exsiting Select Pick
        if (document.querySelector(`[${SELECTPICK_DATA_ID}="${select.id}"]`)) {
            document.querySelector(`[${SELECTPICK_DATA_ID}="${select.id}"]`).remove();
        }
        //Generate Select Pick
        let obj = SelectPick.#data.find(x => x.id == select.id);
        if (!obj) {
            console.error("Select data did not find.");
            return;
        }
        SelectPick.#selectPick(obj);
        //Register Events
        SelectPick.#events();
    }
    static #selectPick(obj) {    
        //Get Select
        let select = document.querySelector(`#${obj.id}`);
        //Create Select Pick Div
        let selectPick = document.createElement('div');
        selectPick.className = SELECTPICK;
        selectPick.setAttribute(SELECTPICK_DATA_ID, obj.id);
        select.insertAdjacentElement('afterend', selectPick);

        //Get Option text
        let optionText = obj.data.filter(op => op.isSelected).map(op => op.text).join(", ");

        //Create Toggle button
        let toggleBtn = document.createElement('button');
        toggleBtn.type = "button";
        toggleBtn.className = SELECTPICK_TOGGLE_BTN;
        toggleBtn.title = `${optionText != "" ? optionText : obj.title}`;
        toggleBtn.disabled = obj.isDisabled;
        toggleBtn.setAttribute(SELECTPICK_TOGGLE_BTN_DATA_TARGET, `#${obj.menuId}`);        
        selectPick.appendChild(toggleBtn);

        //Toggle Button text
        let toggleBtnText = document.createElement('span');
        toggleBtnText.className = SELECTPICK_TOGGLE_BTN_TEXT;
        toggleBtnText.innerHTML = `${optionText != "" ? optionText : obj.title}`;
        toggleBtn.appendChild(toggleBtnText);

        //Toggle Button Icon
        let toggleBtnIcon = document.createElement('span');
        toggleBtnIcon.className = SELECTPICK_TOGGLE_BTN_ICON;
        toggleBtnIcon.innerHTML = "&#9660";
        toggleBtn.appendChild(toggleBtnIcon);

        //Create Select Pick Menu
        let menu = document.createElement('div')
        menu.id = obj.menuId;
        menu.className = SELECTPICK_MENU;
        selectPick.appendChild(menu);

        //Create Search Box
        if (obj.searchBox == "true") {            
            let searchBox = document.createElement('div');
            searchBox.className = SELECTPICK_MENU_SEACRH_BOX;
            menu.appendChild(searchBox);
            //Create Search Box Input
            let searchInput = document.createElement('input');
            searchInput.id = `${obj.id}-SelectPick-Search`;
            searchInput.type = "text";
            searchInput.name = "Select-Pick-Search";
            searchInput.className = "form-control form-control-sm"; //Class from bootstrap
            searchInput.setAttribute('autocomplete', 'off');
            searchBox.appendChild(searchInput);
        }
        
        //Create Action Box
        if (obj.actionBox == "true") {
            let actionBox = document.createElement('div');
            actionBox.className = SELECTPICK_MENU_ACTION_BOX;
            menu.appendChild(actionBox);            
            if (obj.isMultiple) {
                //Select All
                let btnSelectAll = document.createElement('button');
                btnSelectAll.type = "button";
                btnSelectAll.title = "Select All";
                btnSelectAll.className = `${SELECTPICK_MENU_ACTION_SELECTALL} btn btn-sm btn-primary`;
                btnSelectAll.innerHTML = `<i class="fa fa-check"></i>&nbsp;&nbsp;Select All`;
                actionBox.appendChild(btnSelectAll);
                //Deelect All
                let btnDeselectAll = document.createElement('button');
                btnDeselectAll.type = "button";                
                btnDeselectAll.title = "Deselect All";
                btnDeselectAll.className = `${SELECTPICK_MENU_ACTION_DESELECTALL} btn-info-1`;
                btnDeselectAll.innerHTML = `<i class="fa fa-times"></i>&nbsp;&nbsp;Deselect All`;
                actionBox.appendChild(btnDeselectAll);
            }
        }
        if (obj.addFn) {
            let itemAdd = document.createElement('div');
            itemAdd.className = SELECTPICK_MENU_ITEM_ADD;
            menu.appendChild(itemAdd);
            let btnAdd = document.createElement('button');
            btnAdd.type = "button";
            btnAdd.className = `${SELECTPICK_MENU_ACTION_ADDNEW} btn btn-sm btn-success`;
            btnAdd.innerHTML = '<i class="fa fa-plus"></i>&nbsp;&nbsp;Add New';
            btnAdd.title = "Add New";
            btnAdd.setAttribute("onclick", `${obj.addFn}`);
            itemAdd.appendChild(btnAdd);
        }
        //Add Item LIst
        let menuList = document.createElement('ul');
        menuList.className = SELECTPICK_MENU_LIST;
        menu.appendChild(menuList);

        //Add List Item
        SelectPick.#item(obj);
    }
    static #item(obj) {
        let menuList = document.querySelector(`#${obj.menuId} .${SELECTPICK_MENU_LIST}`);
        //Remove All Child
        menuList.innerHTML = '';
        //Append Child
        obj.data.forEach(item => {
            //Create List Item
            let listItem = document.createElement('li');
            listItem.id = item.id;
            listItem.className = item.isSelected ? SELECTPICK_MENU_ITEM + ' ' + SELECTPICK_ITEM_SELECTED : SELECTPICK_MENU_ITEM;
            listItem.tabIndex = 0;
            menuList.appendChild(listItem);
            //Create Item Icon
            if (item.icon) {
                let itemIcon = document.createElement('div');
                itemIcon.className = SELECTPICK_ITEM_ICON;
                itemIcon.innerHTML = `<i class="${item.icon}"></i>`;
                listItem.appendChild(itemIcon);
            }
            //Create Item Text
            let itemText = document.createElement('div');
            itemText.className = SELECTPICK_ITEM_TEXT;
            itemText.innerHTML = item.text;  
            listItem.appendChild(itemText);            
            //Create Sub text
            if (item.subText) {
                let subText = document.createElement('div');
                subText.className = SELECTPICK_ITEM_SUBTEXT;
                subText.innerHTML = item.subText;
                itemText.appendChild(subText);
            }
            //Create Html
            if (item.html) {
                let html = document.createElement('div');
                html.className = SELECTPICK_ITEM_HTML;
                html.innerHTML = item.html;
                itemText.appendChild(html);
            }
            //Create btn group
            let itemBtnGroup = document.createElement('div');
            itemBtnGroup.className = SELECTPICK_ITEM_BTN;
            listItem.appendChild(itemBtnGroup);
            //Create Item Selected Icon Hide or show btn
            let itemSelected = document.createElement('span');
            itemSelected.className = SELECTPICK_ITEM_SELECTED;
            itemSelected.innerHTML = `<i class="fa fa-check"></i>`;                        
            itemBtnGroup.appendChild(itemSelected);
            //Create Edit Btn
            if (item.editFn) {
                let editBtn = document.createElement('button');
                editBtn.type = "button";
                editBtn.className = "btn btn-sm btn-outline-primary";  
                editBtn.innerHTML = `<i class="fa fa-pencil"></i>`;
                editBtn.title = "Edit"
                editBtn.setAttribute('onclick', item.editFn);
                itemBtnGroup.appendChild(editBtn);
            }
            //Create Delete Btn
            if (item.deleteFn) {
                let deleteBtn = document.createElement('button');
                deleteBtn.type = "button";
                deleteBtn.className = "btn btn-sm btn-outline-danger";
                deleteBtn.innerHTML = `<i class="fa fa-trash"></i>`;
                deleteBtn.title = "Delete";
                deleteBtn.setAttribute('onclick', item.deleteFn);
                itemBtnGroup.appendChild(deleteBtn);
            }
            //select option click event register
            listItem.removeEventListener('click', SelectPick.#selectItem);
            listItem.addEventListener('click', SelectPick.#selectItem);
        });        
    }
    static #events() {
        //Toggle button click event for show & hide selectpick list item
        document.querySelectorAll(`.${SELECTPICK_TOGGLE_BTN}`).forEach(btn => {
            btn.removeEventListener('click', SelectPick.#show);
            btn.addEventListener('click', SelectPick.#show);
        });
        //Special Key input on item press escape to close the item, press down arrow to select next item
        document.querySelectorAll(`.${SELECTPICK} .${SELECTPICK_MENU_SEACRH_BOX} input`).forEach((el) => {
            el.removeEventListener('keydown', SelectPick.#specialKey);
            el.addEventListener('keydown', SelectPick.#specialKey);
        });
        //Search Item on the behalf of input text
        document.querySelectorAll(`.${SELECTPICK} .${SELECTPICK_MENU_SEACRH_BOX} input`).forEach((el) => {
            el.removeEventListener('input', SelectPick.#search);
            el.addEventListener('input', SelectPick.#search);
        });
        //Select All Item
        document.querySelectorAll(`.${SELECTPICK} .${SELECTPICK_MENU_ACTION_BOX} button.btn-select-all`).forEach((el) => {
            el.removeEventListener('click', SelectPick.#selectAllItem);
            el.addEventListener('click', SelectPick.#selectAllItem);
        });
        //Deselect All Item
        document.querySelectorAll(`.${SELECTPICK} .${SELECTPICK_MENU_ACTION_BOX} button.btn-deselect-all`).forEach((el) => {
            el.removeEventListener('click', SelectPick.#deSelectAllItem);
            el.addEventListener('click', SelectPick.#deSelectAllItem);
        });        
    }
    static #show(event) {
        let selectPick = event.currentTarget.closest(`.${SELECTPICK}`);
        if (!selectPick) {
            console.error("Select Pick did not find.");
            return;
        }
        if (!selectPick.attributes[SELECTPICK_DATA_ID]) {
            console.error("Select Pick Data Id undefined.");
            return;
        }
        let coordinate = selectPick.getBoundingClientRect();
        let selectId = selectPick.attributes[SELECTPICK_DATA_ID].value;
        let obj = SelectPick.#data.find(x => x.id == selectId);
        if (!obj) {
            console.error(`Select id ${selectId} data did not find.`);
            return;
        }
        //Reset Search Input Box
        if (obj.searchBox) {
            selectPick.querySelector(`.${SELECTPICK_MENU_SEACRH_BOX} input`).setAttribute('name', 'selectPick-Search-Clear');
            selectPick.querySelector(`.${SELECTPICK_MENU_SEACRH_BOX} input`).value = '';
            selectPick.querySelector(`.${SELECTPICK_MENU_SEACRH_BOX} input`).focus();
        }        
        //Reset List Item        
        if (obj.isSearched) {
            SelectPick.#item(obj);
            obj.isSearched = false;
        }
        
        //Select Menu
        let selectPickMenuId = event.currentTarget.attributes[SELECTPICK_TOGGLE_BTN_DATA_TARGET].value;
        if (!selectPickMenuId) {
            console.error(`Select menu did not find for ${selectId}.`);
            return;
        }
        //Change Select Pick Icon Angle
        let selectPickToggleIcon = document.querySelectorAll(`.${SELECTPICK_TOGGLE_BTN}:not([${SELECTPICK_TOGGLE_BTN_DATA_TARGET}="${selectPickMenuId}"]) > .${SELECTPICK_TOGGLE_BTN_ICON}`);
        selectPickToggleIcon.forEach(el => el.classList.remove('up'));
        //Close other select pick
        let selectPickMenu = document.querySelectorAll(`.${SELECTPICK_MENU}.${SELECTPICK_MENU_SHOW}:not(${selectPickMenuId})`);
        selectPickMenu.forEach(el => el.classList.remove(`${SELECTPICK_MENU_SHOW}`));
        //Get Current Select Pick
        selectPickMenu = document.querySelector(`${selectPickMenuId}`);
        selectPickMenu.classList.toggle(SELECTPICK_MENU_SHOW);
        event.currentTarget.querySelector(`.${SELECTPICK_TOGGLE_BTN_ICON}`).classList.toggle('up');
        if (obj.searchBox) {
            selectPick.querySelector(`.${SELECTPICK_MENU_SEACRH_BOX} input`).focus();
        }        
        //Scaling the selectpick
        if (selectPickMenu.classList.contains(SELECTPICK_MENU_SHOW)) {
            SelectPick.scaling(selectPick, coordinate);
        }
        //List Key Action
        document.querySelectorAll(`.${SELECTPICK} li.${SELECTPICK_MENU_ITEM}`).forEach((el) => {
            el.removeEventListener('keydown', SelectPick.#liKeyAction, false);
            el.addEventListener('keydown', SelectPick.#liKeyAction, false);
        });
    }
    static scaling(selectPick, coordinates) {
        let select = document.querySelector(`#${selectPick.attributes['data-id'].value}`);
        let parent = document.querySelector(SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_PARENT, initVal: 'body' }));
        let selectPickMenu = selectPick.querySelector(`.${SELECTPICK_MENU}`);
        //Set Y-Scaling
        let selectPickList = selectPickMenu.lastChild;
        let dataSize = parseInt(SelectPick.#getAttrVal({ el: select, attr: SELECTPICK_DATA_SIZE, initVal: '0' }));
        let selectPickItemHeight = selectPickMenu.lastChild.firstElementChild == null ? 0 : selectPickMenu.lastChild.firstElementChild.offsetHeight;
        selectPickList.style.maxHeight = (dataSize <= 1 ? window.innerHeight / 2 : (dataSize * selectPickItemHeight) + 5) + 'px';

        //Set X-Scaling        
        selectPickMenu.style.removeProperty('maxWidth');
        selectPickMenu.style.removeProperty('minWidth');
        //Set Max & Min Width
        selectPickMenu.style.minWidth = coordinates.width + 'px';
        let maxWidth = parent.offsetWidth > 1000 ? 1000 : parent.offsetWidth - 30;
        selectPickMenu.style.maxWidth = maxWidth + 'px';
        //Set X Transform
        selectPickMenu.style.removeProperty('transform');
        let padding = parent == document.body ? 14 : 10;
        let overflowX = ((Math.ceil(coordinates.x) + selectPickMenu.offsetWidth) - parent.offsetWidth) + padding;
        let translateX = overflowX < 0 ? 0 : overflowX;
        selectPickMenu.style.transform = `translate(-${translateX}px, 0px)`;
        
    }
    static #specialKey(event) {
        if (event.code == "Escape") {
            let selectPickMenu = document.querySelector(`#${event.currentTarget.id}`).closest(`.${SELECTPICK_MENU}`);
            selectPickMenu.classList.remove(SELECTPICK_MENU_SHOW);
            selectPickMenu.previousElementSibling.focus();            
            return;
        }
        if (event.code == "ArrowDown") {
            let menuList = document.querySelector(`#${event.currentTarget.id}`).closest(`.${SELECTPICK_MENU}`).querySelector(`.${SELECTPICK_MENU_LIST}`);
            menuList.firstChild.focus();
            /*menuList.scrollTo({ top: 0, behavior: 'smooth' });*/
            return;
        }        
    }
    static #search(event) {
        let id = document.querySelector(`#${event.currentTarget.id}`).closest(`.${SELECTPICK}`).attributes[SELECTPICK_DATA_ID].value;
        let SelectPickData = SelectPick.#data.find(x => x.id == id);
        if (SelectPickData) {
            SelectPickData.isSearched = true;
        }
        let obj = JSON.parse(JSON.stringify(SelectPickData));//For Deep Copy we have use json parse
        let filterObj = JSON.parse(JSON.stringify(SelectPickData));//For Deep Copy we have use json parse
        filterObj.data = [];
        let input = event.currentTarget.value;
        if (!input) {
            SelectPick.#item(obj);
            return;
        }        
        obj.data.forEach(op => {
            let text = `${op.text} ${op.subText}`;            
            if (text.toLowerCase().includes(input.toLowerCase())) {
                filterObj.data.push(op);                
            }
        });
        SelectPick.#item(filterObj);
        //List Key Action
        document.querySelectorAll(`.${SELECTPICK} li.${SELECTPICK_MENU_ITEM}`).forEach((el) => {
            el.removeEventListener('keydown', SelectPick.#liKeyAction);
            el.addEventListener('keydown', SelectPick.#liKeyAction);
        });
    }
    static #selectItem(event) {
        let id = document.querySelector(`#${event.currentTarget.id}`).closest(`.${SELECTPICK}`).attributes[SELECTPICK_DATA_ID].value;
        let obj = SelectPick.#data.find(x => x.id == id);
        if (!obj.isMultiple) {
            event.currentTarget.parentElement.querySelectorAll(`.${SELECTPICK_MENU_ITEM}.${SELECTPICK_ITEM_SELECTED}:not(#${event.currentTarget.id})`).forEach(op => op.classList.remove(SELECTPICK_ITEM_SELECTED));
            event.currentTarget.closest(`.${SELECTPICK_MENU}`).classList.remove(SELECTPICK_MENU_SHOW);
        }
        event.currentTarget.classList.toggle(SELECTPICK_ITEM_SELECTED);
        let isItemSelected = Array.from(event.currentTarget.classList).filter(cl => cl == SELECTPICK_ITEM_SELECTED).length > 0 ? true : false;
        if (!isItemSelected && !obj.isMultiple) {
            document.querySelector(`#${id}`).value = null;
        }                
        obj.data.forEach(op => {
            if (obj.isMultiple) {
                if (op.id == event.currentTarget.id) {
                    op.isSelected = isItemSelected ? true : false;
                }
            }
            else {
                op.isSelected = op.id == event.currentTarget.id && isItemSelected ? true : false;
            }
        });
        let selectItem = obj.data.filter(x => x.isSelected);
        let text = selectItem.map(x => x.text).join(", ");
        let innerText = (text == '' ? obj.title : text);
        let toggleBtn = document.querySelector(`#${event.currentTarget.id}`).closest(`.${SELECTPICK}`).querySelector(`.${SELECTPICK_TOGGLE_BTN}`);        
        toggleBtn.title = innerText;
        toggleBtn.querySelector(`.${SELECTPICK_TOGGLE_BTN_TEXT}`).innerHTML = innerText;
        //Set Select Value
        let select = document.querySelector(`#${id}`);
        let options = select.options;
        Array.from(options).forEach(op => { op.selected = selectItem.map(x => x.value).includes(op.value) });
        SelectPick.triggerChange(select);        
    }
    static #selectAllItem(event) {
        let selectPick = event.currentTarget.closest(`.${SELECTPICK}`);
        let obj = SelectPick.#data.find(x => x.id == selectPick.attributes["data-id"].value);
        obj.data.map(x => { x.isSelected = true; });
        selectPick.querySelector(`button .${SELECTPICK_TOGGLE_BTN_TEXT}`).innerHTML = obj.data.map(x => x.text).join(', ');
        selectPick.querySelectorAll('li.menu-item').forEach(li => li.classList.add(SELECTPICK_ITEM_SELECTED));
        let select = document.querySelector(`#${selectPick.attributes[SELECTPICK_DATA_ID].value}`);
        Array.from(select.options).forEach(op => { op.selected = true });
        SelectPick.triggerChange(select);
    }
    static #deSelectAllItem(event) {
        let selectPick = event.currentTarget.closest(`.${SELECTPICK}`);
        let obj = SelectPick.#data.find(x => x.id == selectPick.attributes["data-id"].value);
        obj.data.map(x => { x.isSelected = false; });
        selectPick.querySelector(`button .${SELECTPICK_TOGGLE_BTN_TEXT}`).innerHTML = obj.title;
        selectPick.querySelectorAll('li.menu-item').forEach(li => li.classList.remove(SELECTPICK_ITEM_SELECTED));
        let select = document.querySelector(`#${selectPick.attributes[SELECTPICK_DATA_ID].value}`);
        Array.from(select.options).forEach(op => { op.selected = false });
        SelectPick.triggerChange(select);
    }
    static #liKeyAction(event) {
        event.preventDefault();
        if (event.code == "ArrowDown") {
            if (event.currentTarget.nextSibling != null) {
                event.currentTarget.nextSibling.focus();
            }
            else {
                event.currentTarget.parentElement.firstChild.focus();
            }
        }
        else if (event.code == "ArrowUp") {
            if (event.currentTarget.previousSibling != null) {
                event.currentTarget.previousSibling.focus();
            }
            else {
                event.currentTarget.parentElement.lastChild.focus();
            }
        }
        else if (event.code == "Enter") {            
            SelectPick.#selectItem(event);
        }
        else {
            event.currentTarget.parentElement.previousElementSibling.firstChild.focus();
        }
    }
    static refresh({ selector= '' }) {
        if (!selector) {
            console.error('Selector is undefind.');
            return;
        }
        let selectList = document.querySelectorAll(selector);
        selectList.forEach(select => {
            SelectPick.render(select);
        });        
    }
    static set({ id= '', value= [], text=[] }) {        
        value.map(vl => vl == null ? '' : vl.toString());        
        if (!id) {
            console.error("Select id is undefined.");
            return;
        }
        let select = document.querySelector(`${id}`);
        if (select != null) {
            select.value = null;
            if (value.length > 0) {
                Array.from(select.options).forEach(op => op.selected = value.includes(op.value));
            }
            else if (text.length > 0) {
                Array.from(select.options).forEach(op => op.selected = text.includes(op.text));
            }
            SelectPick.refresh({ selector: id });
        }        
    }
    static triggerChange(select) {
        const event = new Event('change', { bubbles: true });
        // Dispatch the event
        select.dispatchEvent(event);
    }
}
//Apply Selectpick on DOM Content loaad.
document.addEventListener('DOMContentLoaded', () => {
    var selectList = document.querySelectorAll(`select.${SELECTPICK}`);
    selectList.forEach(select => {
        let inputGroup = select.parentElement;
        if (inputGroup.classList.contains('input-group')) {
            inputGroup.style.flexWrap = 'nowrap';
        }        
        SelectPick.render(select);
    });    
});
//Adjust Selectpick size on screen resize
window.addEventListener('resize', (event) => {
    document.querySelectorAll(`div.${SELECTPICK}`).forEach(el => SelectPick.scaling(el, el.getBoundingClientRect()));
}, true);
//Hide Selectpick where click on out side of select dropdown
document.addEventListener('click', (e) => {
    if (e.target.closest(`.${SELECTPICK}`) === null) {
        document.querySelectorAll(`.${SELECTPICK_TOGGLE_BTN_ICON}`).forEach(el => el.classList.remove('up'));
        document.querySelectorAll(`.${SELECTPICK_MENU}.${SELECTPICK_MENU_SHOW}`).forEach(el => el.classList.remove(SELECTPICK_MENU_SHOW));
    }
});

