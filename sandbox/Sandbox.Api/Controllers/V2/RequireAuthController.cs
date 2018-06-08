﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sandbox.Api.Routing;
using Serilog;

namespace Sandbox.Api.Controllers.V2
{
    [VersionedRoute("[controller]"), V2]
    [ApiController]
    public class RequireAuthController : Controller
    {
        private readonly ILogger _logger = Log.ForContext<RequireAuthController>();

        [HttpGet]
        [Authorize(Constants.Auth.Policies.ReadPolicy)]
        public ActionResult<Entity> Get(int id)
        {
            _logger.Information("Should add Subject Property");

            if (id == 0)
            {
                return NotFound();
            }

            return new Entity { Id = id, Name = "Prospa" };
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [Authorize(Constants.Auth.Policies.WritePolicy)]
        public ActionResult<Entity> Post(Entity entity)
        {
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class Entity
#pragma warning restore SA1402 // File may only contain a single class
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
