String.prototype.trimHttpProtocol = function () {
    return this.toLowerCase().trimStart("http://").trimStart("https://");
};
//# sourceMappingURL=extensions.js.map