/**
 * WebExplorer - 局域网文件传输工具
 * 前端主应用逻辑
 */

(function () {
    'use strict';

    // ========================================
    // 状态管理
    // ========================================
    const state = {
        currentPath: '',
        history: [],        // 导航历史栈
        historyIndex: -1,   // 当前历史位置
        selectedItems: new Set(),
        viewMode: 'grid',   // grid | list
        isLoading: false,
        contextTarget: null // 右键菜单目标
    };

    // ========================================
    // DOM 元素引用
    // ========================================
    const $ = (sel) => document.querySelector(sel);
    const $$ = (sel) => document.querySelectorAll(sel);

    const elements = {
        sidebar: $('#sidebar'),
        sidebarOverlay: $('#sidebarOverlay'),
        sidebarClose: $('#sidebarClose'),
        menuBtn: $('#menuBtn'),
        quickAccessList: $('#quickAccessList'),
        drivesList: $('#drivesList'),
        breadcrumb: $('#breadcrumb'),
        addressInput: $('#addressInput'),
        addressBar: $('#addressBar'),
        filesContainer: $('#filesContainer'),
        loadingState: $('#loadingState'),
        emptyState: $('#emptyState'),
        contentArea: $('#contentArea'),
        itemCount: $('#itemCount'),
        selectedInfo: $('#selectedInfo'),
        currentPathDisplay: $('#currentPathDisplay'),
        btnBack: $('#btnBack'),
        btnForward: $('#btnForward'),
        btnUp: $('#btnUp'),
        btnRefresh: $('#btnRefresh'),
        btnNewFolder: $('#btnNewFolder'),
        btnDelete: $('#btnDelete'),
        fileInput: $('#fileInput'),
        uploadLabel: $('#uploadLabel'),
        searchInput: $('#searchInput'),
        contextMenu: $('#contextMenu'),
        toastContainer: $('#toastContainer'),
        uploadPanel: $('#uploadPanel'),
        uploadPanelClose: $('#uploadPanelClose'),
        uploadList: $('#uploadList')
    };

    // ========================================
    // 工具函数
    // ========================================
    function formatFileSize(bytes) {
        if (!bytes || bytes === 0) return '-';
        const units = ['B', 'KB', 'MB', 'GB', 'TB'];
        let size = bytes;
        let unitIndex = 0;
        while (size >= 1024 && unitIndex < units.length - 1) {
            size /= 1024;
            unitIndex++;
        }
        return size.toFixed(unitIndex > 0 ? 1 : 0) + ' ' + units[unitIndex];
    }

    function escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    // 修正盘符根路径：C: → C:\，避免 Windows 解释为驱动器相对路径
    function fixDriveRoot(path) {
        return /^[a-zA-Z]:$/.test(path) ? path + '\\' : path;
    }

    function getFileIcon(item) {
        if (item.isDirectory) {
            return `<svg viewBox="0 0 24 24" fill="currentColor" class="icon-folder">
                <path d="M10 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z"/>
            </svg>`;
        }

        const ext = item.extension.toLowerCase();
        const iconMap = {
            image: ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg', '.ico'],
            video: ['.mp4', '.avi', '.mkv', '.mov', '.wmv', '.flv', '.3gp'],
            audio: ['.mp3', '.wav', '.flac', '.aac', '.ogg', '.wma', '.m4a'],
            doc: ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx', '.txt'],
            zip: ['.zip', '.rar', '.7z', '.tar', '.gz'],
            code: ['.js', '.ts', '.html', '.css', '.json', '.xml', '.py', '.java', '.cs', '.cpp', '.c', '.h']
        };

        let iconClass = 'icon-file-generic';
        for (const [cls, exts] of Object.entries(iconMap)) {
            if (exts.includes(ext)) {
                iconClass = `icon-file-${cls}`;
                break;
            }
        }

        return `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="${iconClass}">
            <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8l-6-6z"/>
            <polyline points="14 2 14 8 20 8"/>
        </svg>`;
    }

    function showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.textContent = message;
        elements.toastContainer.appendChild(toast);
        setTimeout(() => toast.remove(), 3000);
    }

    // ========================================
    // API 调用
    // ========================================
    async function apiGet(url) {
        try {
            const res = await fetch(url);
            return await res.json();
        } catch (err) {
            console.error('API Error:', err);
            showToast('网络请求失败', 'error');
            return null;
        }
    }

    async function apiPost(url, body) {
        try {
            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            return await res.json();
        } catch (err) {
            console.error('API Error:', err);
            showToast('请求失败', 'error');
            return null;
        }
    }

    async function apiDelete(url, body) {
        try {
            const res = await fetch(url, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            return await res.json();
        } catch (err) {
            console.error('API Error:', err);
            showToast('请求失败', 'error');
            return null;
        }
    }

    // ========================================
    // 导航与文件浏览
    // ========================================
    async function navigateTo(path, pushHistory = true) {
        state.isLoading = true;
        showLoading(true);

        const data = await apiGet(`/api/files?path=${encodeURIComponent(path)}`);

        if (!data || !data.success) {
            showToast(data?.message || '加载目录失败', 'error');
            showLoading(false);
            state.isLoading = false;
            return;
        }

        // 更新状态
        if (pushHistory) {
            // 如果不在历史末尾，截断后续历史
            if (state.historyIndex < state.history.length - 1) {
                state.history = state.history.slice(0, state.historyIndex + 1);
            }
            state.history.push(path);
            state.historyIndex = state.history.length - 1;
        }

        state.currentPath = data.data.path;
        state.selectedItems.clear();

        // 更新 UI
        renderBreadcrumb(data.data.path);
        renderFiles(data.data.items);
        updateNavButtons();
        updateStatusBar(data.data.items.length);
        updateAddressBar(data.data.path);

        showLoading(false);
        state.isLoading = false;

        // 关闭移动端侧边栏
        closeSidebar();
    }

    function resolvePath(el) {
        return el.dataset.path || '';
    }

    function renderBreadcrumb(path) {
        // 解析路径为段（保留原始 \ 分隔）
        const parts = path.split(/[/\\]/).filter(Boolean);
        let html = '';

        parts.forEach((part, index) => {
            let fullPath = fixDriveRoot(parts.slice(0, index + 1).join('\\'));
            const isLast = index === parts.length - 1;

            if (index > 0) {
                html += `<span class="breadcrumb-sep">›</span>`;
            }

            html += `<span class="breadcrumb-item${isLast ? '' : '-clickable'}"
                        data-nav-path="${escapeHtml(fullPath)}"
                        ${isLast ? '' : 'role="button"'}>
                      ${escapeHtml(part)}
                    </span>`;
        });

        elements.breadcrumb.innerHTML = html;
    }

    function renderFiles(items) {
        if (items.length === 0) {
            elements.filesContainer.innerHTML = '';
            elements.emptyState.style.display = 'flex';
            elements.itemCount.textContent = '0 个项目';
            return;
        }

        elements.emptyState.style.display = 'none';

        if (state.viewMode === 'grid') {
            elements.filesContainer.className = 'files-container grid-view';
            elements.filesContainer.innerHTML = items.map(item => `
                <div class="file-item-grid"
                     data-path="${escapeHtml(item.fullPath)}"
                     data-name="${escapeHtml(item.name)}"
                     data-isdir="${item.isDirectory}">
                    <div class="file-icon">${getFileIcon(item)}</div>
                    <div class="file-name">${escapeHtml(item.name)}</div>
                </div>
            `).join('');
        } else {
            elements.filesContainer.className = 'files-container list-view';
            elements.filesContainer.innerHTML = items.map(item => `
                <div class="file-item-list"
                     data-path="${escapeHtml(item.fullPath)}"
                     data-name="${escapeHtml(item.name)}"
                     data-isdir="${item.isDirectory}">
                    <div class="file-list-info">
                        <div class="file-list-icon">${getFileIcon(item)}</div>
                        <div class="file-list-name">${escapeHtml(item.name)}</div>
                    </div>
                    <div class="file-meta">${formatFileSize(item.size)}</div>
                    <div class="file-meta">${item.lastModified}</div>
                    <div class="file-meta">${item.isDirectory ? '文件夹' : item.extension.toUpperCase() || '文件'}</div>
                </div>
            `).join('');
        }

        bindFileItemEvents();
    }

    function bindFileItemEvents() {
        const items = $$('.file-item-grid, .file-item-list');

        items.forEach(el => {
            // 单击选择
            el.addEventListener('click', (e) => {
                e.stopPropagation();
                toggleSelection(el);
            });

            // 双击导航/下载
            el.addEventListener('dblclick', (e) => {
                e.stopPropagation();
                const path = resolvePath(el);
                if (el.dataset.isdir === 'true') {
                    navigateTo(path);
                } else {
                    downloadFile(path);
                }
            });

            // 长按（移动端）
            let pressTimer;
            el.addEventListener('touchstart', (e) => {
                pressTimer = setTimeout(() => {
                    el.classList.add('selected');
                    state.selectedItems.add(resolvePath(el));
                    updateSelectionUI();
                    showContextMenu(e.touches[0].clientX, e.touches[0].clientY, el);
                }, 500);
            });
            el.addEventListener('touchend', () => clearTimeout(pressTimer));
            el.addEventListener('touchmove', () => clearTimeout(pressTimer));

            // 右键菜单
            el.addEventListener('contextmenu', (e) => {
                e.preventDefault();
                el.classList.add('selected');
                state.selectedItems.add(resolvePath(el));
                updateSelectionUI();
                showContextMenu(e.clientX, e.clientY, el);
            });
        });
    }

    function toggleSelection(el) {
        const path = resolvePath(el);

        if (state.selectedItems.has(path)) {
            state.selectedItems.delete(path);
            el.classList.remove('selected');
        } else {
            // 单选模式：清除其他选择
            clearSelection();
            state.selectedItems.add(path);
            el.classList.add('selected');
        }

        updateSelectionUI();
    }

    function clearSelection() {
        state.selectedItems.clear();
        $$('.file-item-grid.selected, .file-item-list.selected').forEach(el => {
            el.classList.remove('selected');
        });
    }

    function updateSelectionUI() {
        const count = state.selectedItems.size;
        elements.btnDelete.style.display = count > 0 ? 'flex' : 'none';
        elements.selectedInfo.textContent = count > 0 ? `${count} 项已选中` : '';
    }

    function updateNavButtons() {
        elements.btnBack.disabled = state.historyIndex <= 0;
        elements.btnForward.disabled = state.historyIndex >= state.history.length - 1;
        elements.btnUp.disabled = !state.currentPath || /^[a-zA-Z]:\\$/.test(state.currentPath);
    }

    function updateStatusBar(count) {
        elements.itemCount.textContent = `${count} 个项目`;
        elements.currentPathDisplay.textContent = state.currentPath || '';
    }

    function updateAddressBar(path) {
        elements.addressInput.value = path;
    }

    function showLoading(show) {
        elements.loadingState.style.display = show ? 'flex' : 'none';
        elements.filesContainer.style.display = show ? 'none' : '';
    }

    // ========================================
    // 右键菜单
    // ========================================
    function showContextMenu(x, y, targetEl) {
        state.contextTarget = targetEl;

        const menu = elements.contextMenu;
        menu.style.display = 'block';

        // 边界检测
        const rect = menu.getBoundingClientRect();
        const winW = window.innerWidth;
        const winH = window.innerHeight;

        menu.style.left = Math.min(x, winW - rect.width - 8) + 'px';
        menu.style.top = Math.min(y, winH - rect.height - 8) + 'px';
    }

    function hideContextMenu() {
        elements.contextMenu.style.display = 'none';
        state.contextTarget = null;
    }

    // ========================================
    // 上传功能
    // ========================================
    async function handleUpload(files) {
        if (!files || files.length === 0) return;

        elements.uploadPanel.style.display = 'block';

        for (const file of files) {
            await uploadSingleFile(file);
        }
    }

    async function uploadSingleFile(file) {
        const itemEl = document.createElement('div');
        itemEl.className = 'upload-item';
        itemEl.innerHTML = `
            <div class="upload-item-header">
                <span class="upload-item-name" title="${escapeHtml(file.name)}">${escapeHtml(file.name)}</span>
                <span class="upload-item-status">准备中...</span>
            </div>
            <div class="progress-bar"><div class="progress-fill" style="width:0%"></div></div>
        `;
        elements.uploadList.appendChild(itemEl);

        const progressFill = itemEl.querySelector('.progress-fill');
        const statusEl = itemEl.querySelector('.upload-item-status');

        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('targetPath', state.currentPath);

            statusEl.textContent = '上传中...';

            const xhr = new XMLHttpRequest();

            await new Promise((resolve, reject) => {
                xhr.upload.onprogress = (e) => {
                    if (e.lengthComputable) {
                        const pct = Math.round((e.loaded / e.total) * 100);
                        progressFill.style.width = pct + '%';
                        statusEl.textContent = `${pct}%`;
                    }
                };

                xhr.onload = () => {
                    if (xhr.status === 200) {
                        progressFill.style.width = '100%';
                        statusEl.textContent = '完成 ✓';
                        resolve();
                    } else {
                        reject(new Error(`HTTP ${xhr.status}`));
                    }
                };

                xhr.onerror = () => reject(new Error('网络错误'));
                xhr.open('POST', '/api/upload');
                xhr.send(formData);
            });

            showToast(`${file.name} 上传成功`, 'success');

            // 刷新当前目录
            navigateTo(state.currentPath, false);

        } catch (err) {
            progressFill.style.background = '#c42b1c';
            statusEl.textContent = '失败 ✗';
            console.error('Upload error:', err);
            showToast(`上传失败: ${file.name}`, 'error');
        }
    }

    // ========================================
    // 下载功能
    // ========================================
    function downloadFile(path) {
        const link = document.createElement('a');
        link.href = `/api/download?path=${encodeURIComponent(path)}`;
        link.click();
    }

    // ========================================
    // 删除功能
    // ========================================
    async function deleteSelected() {
        if (state.selectedItems.size === 0) return;

        const count = state.selectedItems.size;
        if (!confirm(`确定要删除选中的 ${count} 项吗？此操作不可撤销。`)) return;

        let successCount = 0;
        for (const path of state.selectedItems) {
            const result = await apiDelete('/api/delete', { path });
            if (result?.success) successCount++;
        }

        if (successCount > 0) {
            showToast(`已删除 ${successCount} 项`, 'success');
            navigateTo(state.currentPath, false);
        }
    }

    // ========================================
    // 新建文件夹
    // ========================================
    async function createNewFolder() {
        const name = prompt('请输入文件夹名称：');
        if (!name || !name.trim()) return;

        const result = await apiPost('/api/newfolder', {
            path: state.currentPath,
            name: name.trim()
        });

        if (result?.success) {
            showToast('文件夹创建成功', 'success');
            navigateTo(state.currentPath, false);
        } else {
            showToast(result?.message || '创建失败', 'error');
        }
    }

    // ========================================
    // 侧边栏
    // ========================================
    function openSidebar() {
        elements.sidebar.classList.add('open');
        elements.sidebarOverlay.classList.add('show');
    }

    function closeSidebar() {
        elements.sidebar.classList.remove('open');
        elements.sidebarOverlay.classList.remove('show');
    }

    // ========================================
    // 加载侧边栏数据
    // ========================================
    async function loadQuickAccess() {
        const data = await apiGet('/api/quickaccess');
        if (!data?.success) return;

        const iconSvgs = {
            desktop: '<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="3" width="20" height="14" rx="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/></svg>',
            documents: '<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8l-6-6z"/><polyline points="14 2 14 8 20 8"/></svg>',
            download: '<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>',
            pictures: '<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="m21 15-5-5L5 21"/></svg>',
            videos: '<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><polygon points="23 7 16 12 23 17 23 7"/><rect x="1" y="5" width="15" height="14" rx="2"/></svg>',
            music: '<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 18V5l12-2v13"/><circle cx="6" cy="18" r="3"/><circle cx="18" cy="16" r="3"/></svg>'
        };

        elements.quickAccessList.innerHTML = data.data.map(item => `
            <li>
                <button class="nav-item" data-nav-path="${escapeHtml(item.path)}">
                    <span class="nav-icon-${item.icon}">${iconSvgs[item.icon] || iconSvgs.documents}</span>
                    ${escapeHtml(item.name)}
                </button>
            </li>
        `).join('');
    }

    async function loadDrives() {
        const data = await apiGet('/api/drives');
        if (!data?.success) return;

        elements.drivesList.innerHTML = data.data.map(drive => {
            const usedPct = drive.totalSpace > 0 ? ((drive.usedSpace / drive.totalSpace) * 100).toFixed(0) : 0;
            return `
                <li>
                    <button class="nav-item" data-nav-path="${drive.letter}\\">
                        <span class="nav-icon-drive">
                            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
                                <ellipse cx="12" cy="5" rx="9" ry="3"/>
                                <path d="M21 12c0 1.66-4 3-9 3s-9-1.34-9-3"/>
                                <path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"/>
                            </svg>
                        </span>
                        <span>${escapeHtml(drive.label)} (${drive.letter})</span>
                        <span class="drive-info">${usedPct}%</span>
                    </button>
                </li>
            `;
        }).join('');
    }

    // ========================================
    // 视图切换
    // ========================================
    function setViewMode(mode) {
        state.viewMode = mode;
        $$('.view-btn').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.view === mode);
        });

        // 重新渲染当前文件列表
        if (state.currentPath && !state.isLoading) {
            navigateTo(state.currentPath, false);
        }
    }

    // ========================================
    // 地址栏交互
    // ========================================
    function enableAddressEdit() {
        elements.breadcrumb.style.display = 'none';
        elements.addressInput.style.display = 'block';
        elements.addressInput.focus();
        elements.addressInput.select();
    }

    function disableAddressEdit() {
        elements.breadcrumb.style.display = 'flex';
        elements.addressInput.style.display = 'none';

        const newPath = elements.addressInput.value.trim();
        if (newPath && newPath !== state.currentPath) {
            navigateTo(newPath);
        }
    }

    // ========================================
    // 事件绑定
    // ========================================
    function bindEvents() {
        // 侧边栏开关
        elements.menuBtn.addEventListener('click', openSidebar);
        elements.sidebarClose.addEventListener('click', closeSidebar);
        elements.sidebarOverlay.addEventListener('click', closeSidebar);

        // 导航按钮
        elements.btnBack.addEventListener('click', () => {
            if (state.historyIndex > 0) {
                state.historyIndex--;
                navigateTo(state.history[state.historyIndex], false);
            }
        });

        elements.btnForward.addEventListener('click', () => {
            if (state.historyIndex < state.history.length - 1) {
                state.historyIndex++;
                navigateTo(state.history[state.historyIndex], false);
            }
        });

        elements.btnUp.addEventListener('click', () => {
            if (state.currentPath) {
                const parentPath = state.currentPath.replace(/[/\\][^/\\]+$/, '') || state.currentPath.split(/[\\/]/)[0] + '\\';
                navigateTo(fixDriveRoot(parentPath));
            }
        });

        elements.btnRefresh.addEventListener('click', () => {
            navigateTo(state.currentPath, false);
        });

        // 地址栏
        elements.addressBar.addEventListener('dblclick', enableAddressEdit);
        elements.addressInput.addEventListener('blur', disableAddressEdit);
        elements.addressInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') disableAddressEdit();
            if (e.key === 'Escape') {
                elements.addressInput.value = state.currentPath;
                disableAddressEdit();
            }
        });

        // 视图切换
        $$('.view-btn').forEach(btn => {
            btn.addEventListener('click', () => setViewMode(btn.dataset.view));
        });

        // 操作按钮
        elements.btnNewFolder.addEventListener('click', createNewFolder);
        elements.btnDelete.addEventListener('click', deleteSelected);

        // 文件上传
        elements.fileInput.addEventListener('change', (e) => {
            handleUpload(Array.from(e.target.files));
            e.target.value = ''; // 清空以便重复选择
        });

        // 拖拽上传
        elements.contentArea.addEventListener('dragover', (e) => {
            e.preventDefault();
            elements.contentArea.classList.add('drag-over');
        });

        elements.contentArea.addEventListener('dragleave', (e) => {
            e.preventDefault();
            elements.contentArea.classList.remove('drag-over');
        });

        elements.contentArea.addEventListener('drop', (e) => {
            e.preventDefault();
            elements.contentArea.classList.remove('drag-over');
            const files = Array.from(e.dataTransfer.files);
            handleUpload(files);
        });

        // 右键菜单操作
        $$('.context-item').forEach(item => {
            item.addEventListener('click', () => {
                const action = item.dataset.action;
                const target = state.contextTarget;

                if (action === 'download' && target) {
                    downloadFile(resolvePath(target));
                } else if (action === 'delete' && target) {
                    state.selectedItems.add(resolvePath(target));
                    updateSelectionUI();
                    deleteSelected();
                }

                hideContextMenu();
            });
        });

        // 点击空白处关闭右键菜单
        document.addEventListener('click', (e) => {
            if (!elements.contextMenu.contains(e.target)) {
                hideContextMenu();
            }
        });

        // 点击内容区空白处取消选择
        elements.contentArea.addEventListener('click', (e) => {
            if (e.target === elements.contentArea || e.target === elements.filesContainer) {
                clearSelection();
                updateSelectionUI();
            }
        });

        // 上传面板关闭
        elements.uploadPanelClose.addEventListener('click', () => {
            elements.uploadPanel.style.display = 'none';
        });

        // 键盘快捷键
        document.addEventListener('keydown', (e) => {
            // Delete 键删除
            if (e.key === 'Delete' && state.selectedItems.size > 0) {
                deleteSelected();
            }
            // F5 刷新
            if (e.key === 'F5') {
                e.preventDefault();
                navigateTo(state.currentPath, false);
            }
            // Backspace 返回上级
            if (e.key === 'Backspace' && !e.target.matches('input')) {
                e.preventDefault();
                elements.btnUp.click();
            }
            // Escape 取消选择
            if (e.key === 'Escape') {
                clearSelection();
                updateSelectionUI();
                hideContextMenu();
            }
        });

        // ====== 事件委托：面包屑导航点击 ======
        elements.breadcrumb.addEventListener('click', (e) => {
            const navItem = e.target.closest('[data-nav-path]');
            if (navItem) {
                navigateTo(navItem.dataset.navPath);
            }
        });

        // ====== 事件委托：侧边栏导航（快速访问 + 驱动器） ======
        document.addEventListener('click', (e) => {
            const navBtn = e.target.closest('[data-nav-path]');
            if (navBtn && (elements.quickAccessList.contains(navBtn) || elements.drivesList.contains(navBtn))) {
                navigateTo(navBtn.dataset.navPath);
            }
        });
    }

    // ========================================
    // 初始化
    // ========================================
    async function init() {
        bindEvents();

        // 加载侧边栏数据
        await Promise.all([loadQuickAccess(), loadDrives()]);

        // 默认导航到用户目录或第一个驱动器
        const drivesData = await apiGet('/api/drives');
        if (drivesData?.success && drivesData.data.length > 0) {
            navigateTo(drivesData.data[0].letter + '\\');
        } else {
            // 回退到用户配置目录
            navigateTo('C:\\');
        }
    }

    // 启动应用
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
