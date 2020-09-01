using System;
using Cybtans.Tests.Clients;
using Cybtans.Tests.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cybtans.AspNetCore;

namespace Gateway.Controllers.Cybtans.Tests
{
	[Route("api/Order")]
	[ApiController]
	public partial class OrderServiceController : ControllerBase
	{
		private readonly IOrderService _service;
		
		public OrderServiceController(IOrderService service)
		{
			_service = service;
		}
		
		[HttpGet("foo")]
		public Task Foo()
		{
			return _service.Foo();
		}
		
		[HttpGet("baar")]
		public Task Baar()
		{
			return _service.Baar();
		}
		
		[HttpGet("test")]
		public Task Test()
		{
			return _service.Test();
		}
		
		[HttpGet("arg")]
		public Task Argument()
		{
			return _service.Argument();
		}
		
		[HttpPost("upload")]
		[DisableFormValueModelBinding]
		public Task<UploadImageResponse> UploadImage([ModelBinder(typeof(CybtansModelBinder))]UploadImageRequest __request)
		{
			return _service.UploadImage(__request);
		}
		
		[HttpPost("{id}/upload")]
		[DisableFormValueModelBinding]
		public Task<UploadStreamResponse> UploadStreamById(string id, [ModelBinder(typeof(CybtansModelBinder))]UploadStreamByIdRequest __request)
		{
			__request.Id = id;
			return _service.UploadStreamById(__request);
		}
		
		[HttpPost("stream")]
		[DisableFormValueModelBinding]
		public Task<UploadStreamResponse> UploadStream([ModelBinder(typeof(CybtansModelBinder))]System.IO.Stream __request)
		{
			return _service.UploadStream(__request);
		}
		
		[HttpGet("download")]
		public async Task<IActionResult> DownloadImage([FromQuery]DownloadImageRequest __request)
		{
			var stream = await _service.DownloadImage(__request);
			return new FileStreamResult(stream, "image/jpg") { FileDownloadName = "Image.jpg" };
		}
		
		[HttpGet]
		public Task<GetAllOrderResponse> GetAll([FromQuery]GetAllRequest __request)
		{
			return _service.GetAll(__request);
		}
		
		[HttpGet("{id}")]
		public Task<OrderDto> Get(Guid id, [FromQuery]GetOrderRequest __request)
		{
			__request.Id = id;
			return _service.Get(__request);
		}
		
		[HttpPost]
		public Task<OrderDto> Create([FromBody]OrderDto __request)
		{
			return _service.Create(__request);
		}
		
		[HttpPut("{id}")]
		public Task<OrderDto> Update(Guid id, [FromBody]UpdateOrderRequest __request)
		{
			__request.Id = id;
			return _service.Update(__request);
		}
		
		[HttpDelete("{id}")]
		public Task Delete(Guid id, [FromQuery]DeleteOrderRequest __request)
		{
			__request.Id = id;
			return _service.Delete(__request);
		}
	}

}