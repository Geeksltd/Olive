using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PushExpireClientCache : Attribute
    {
    }
}