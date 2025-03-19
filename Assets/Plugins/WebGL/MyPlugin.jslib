var MyPlugin = {
    // 获取URL参数
    GetUrlParams: function() {
        var returnStr = window.location.search;
        var length = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(length);
        stringToUTF8(returnStr, buffer, length);
        return buffer;
    },

    // 释放内存
    FreeUrlParams: function(buffer) {
        _free(buffer);
    },

    // 声明依赖的Emscripten函数
    $MyPlugin: { deps: ['malloc', 'stringToUTF8', 'lengthBytesUTF8', 'free'] }
};

mergeInto(LibraryManager.library, MyPlugin);