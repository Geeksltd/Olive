# BLOB

## What is BLOB

**Blob** (Binary Large OBject)  is a large object data type in the database system. BLOB could store a large chunk of data, document types and even media files like audio or video files. BLOB fields allocate space only whenever the content in the field is utilized. BLOB allocates spaces in Giga Bytes. As they are *unstructured data* in terms of data engineerng and they can be in any size (up to tens of gigabytes), database and application must act in a *special* way with this files.

## Blob in Olive.Entities

**Olive.Entities** support Blob types and provide ways to interact with this type.

### Get started with blobs

Yu can create an instance of a blob by using this method:

```csharp
var someData = new Blob( "e:\\Somewhere.csv" );
```

Where you can pass the file location of that blob. Or you might like to create an empty blob and then insert the binary value into it:

```csharp
var someData = new Blob();
someData.SetData(content);

//Read the content of blob

```

In Olive.entities these type of Blobs are marked as **unsafe** types due to their executive behaviour:

- aspx
- ascx
- ashx
- axd
- master
- bat
- bas
- asp
- app
- bin
- cla
- class
- cmd
- com
- sitemap
- skin
- asa
- cshtml
- cpl
- crt
- csc
- dll
- drv
- exe
- hta
- htm
- html
- ini
- ins
- js
- jse
- lnk
- mdb
- mde
- mht
- mhtm
- mhtml
- msc
- msi
- msp
- mdb
- ldb
- resources
- resx
- mst
- obj
- config
- ocx
- pgm
- pif
- scr
- sct
- shb
- shs
- smm
- sys
- url
- vb
- vbe
- vbs
- vxd
- wsc
- wsf
- wsh
- php
- asmx
- cs
- jsl
- asax
- mdf
- cdx
- idc
- shtm
- shtml
- stm
- browser