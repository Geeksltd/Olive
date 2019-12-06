interface String {
    trimHttpProtocol(): string;
}

String.prototype.trimHttpProtocol = function(): string {
    return this.toLowerCase().trimStart("http://").trimStart("https://");
}
