# BLOB

## What is BLOB

**Blob** (Binary Large OBject)  is a large object data type in the database system. BLOB could store a large chunk of data, document types and even media files like audio or video files. BLOB fields allocate space only whenever the content in the field is utilized. BLOB allocates spaces in Giga Bytes. As they are *unstructured data* in terms of data engineerng and they can be in any size (up to tens of gigabytes), database and application must act in a *special* way with this files.

## Blob in Olive.Entities

**Olive.Entities** supports Blob and provides ways to interact with this type.

In [M# toturial episode 11](https://learn.msharp.co.uk/#/Tutorials/11/README) there is a toturial with sample code that helps you to *securely*. This *SecureFile* Takes advantage of Blob and saves the files in a *Blob* format into the *Blob provider*. Olive saves the Blobs into a *Local Storage prvider* but you can take the advantage of *AWS* and *Azure blob storage* as well.

### How Olive deals with blobs

Let's dive more into Olive Blobs and see how does it really work (make sure that you fully reviewed  [M# toturial's episode 11](https://learn.msharp.co.uk/#/Tutorials/11/README) ).

After you make an entity with a the type of *SecureFile*, M# produces a property with the type of *Blob* in the related class. What we expect to do is to save file somewhere. But after you query the database you'll notice that just a *FileName* is saved! Nothing more.
![image](https://user-images.githubusercontent.com/22152065/37619295-53916c5c-2bce-11e8-9799-3682efb5a556.png)

instead of saving file path or the file itself, Olive uses Entity Id and filename in order to access the resource within the blob provider.

>**Offtopic Note:** NEVER EVER think of saving binaries or blobs in relational databases. it's a really bad practice and slowes your speed down as a minimum effect.

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