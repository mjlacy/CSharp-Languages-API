using CSharp_Languages_API.Models;
using CSharp_Languages_API.Services;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace CSharp_Languages_API.Controllers;

[ApiController]
[Route("")]
public class LanguagesController(LanguagesService languagesService) : ControllerBase {
    private readonly LanguagesService _languagesService = languagesService;

    [HttpGet("health")]
    public ActionResult HealthCheckHandler() {
        HealthCheck hc = new HealthCheck {
            HealthCodes = new HealthCodes { Application = "OK", MongoConnection = "OK" },
            Info = new Info { ApplicationName = Constants.AppName, Version = Constants.Version }
        };

        try {
            if (_languagesService.Ping()) {
                return Ok(hc);
            } else {
                hc.HealthCodes.MongoConnection = "Internal Server Error";

                return StatusCode((int)HttpStatusCode.InternalServerError, hc);
            }
        } catch (OperationCanceledException oce) {
            hc.HealthCodes.MongoConnection = "Internal Server Error";

            return StatusCode((int)HttpStatusCode.InternalServerError, hc);
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred processing this request");
        }
    }
    
    [HttpGet]
    public ActionResult GetLanguagesHandler(string? name, [FromQuery] string? creators,
        string? extensions, DateTime? firstAppeared, int year, string? wiki) {
        try {
            if (Request.QueryString.ToString().Contains('?') && Request.QueryString.ToString().Contains('=')) {
                string key = Request.QueryString.ToString().Split('=')[0].TrimStart('?');
                if (!typeof(Language).GetProperties().ToList().ConvertAll<string>(info => info.Name).Contains(key, StringComparer.OrdinalIgnoreCase)) {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid query string");
                }
            }
            
            string[] creatorsArray = [];
            if (creators != null && creators.Length > 0) {
                creatorsArray = creators.Split(",");
            }

            string[] extensionsArray = [];
            if (extensions != null && extensions.Length > 0) {
                extensionsArray = extensions.Split(",");
            }

            Language language = new Language {
                Name = name,
                Creators = creatorsArray,
                Extensions = extensionsArray,
                FirstAppeared = firstAppeared,
                Year = year,
                Wiki = wiki
            };

            Languages languages = _languagesService.Find(language);

            return Ok(languages);
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred processing this request");
        }
    }

    [HttpGet("{id}")]
    public ActionResult<Language> Get(string id) {
        try {
            Language? language = _languagesService.FindOne(id);

            if (language is null) {
                return NotFound("No language found with that id");
            } else if (language.Id == ResponseCode.InvalidId.ToString()) {
                return BadRequest("The given id is not a valid id");
            }

            return Ok(language);
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred processing this request");
        }
    }
    
    [HttpPost]
    public ActionResult Post() {
        try
        {
            Language? language;

            using (StreamReader reader = new StreamReader(Request.Body))
            {
                language = JsonConvert.DeserializeObject<Language>(reader.ReadToEnd());

                if (language is null)
                {
                    return BadRequest("Invalid request body");
                }
            }

            string id = _languagesService.InsertOne(language);

            Response.Headers.Location = $"/{id}";

            return StatusCode((int)HttpStatusCode.Created);
        } catch (JsonReaderException jex) {
            Console.WriteLine("JsonReaderException occurred processing this request");
            Console.WriteLine(jex.Message);
            return BadRequest("Invalid request body");
        } catch (Exception ex) {
            Console.WriteLine("Unknown exception occurred processing this request");
            Console.WriteLine(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred processing this request");
        }
    }
    
    [HttpPut("{id}")]
    public ActionResult Replace(string id) {
        try {
            Language? language;

            using (StreamReader reader = new StreamReader(Request.Body)) {
                language = JsonConvert.DeserializeObject<Language>(reader.ReadToEnd());

                if (language is null) {
                    return BadRequest("Invalid request body");
                }
            }

            string? respCode = _languagesService.ReplaceOne(id, language);

            if (respCode == ResponseCode.InvalidId.ToString()) {
                return BadRequest("The given id is not a valid id");
            } else if (respCode == ResponseCode.Found.ToString()) {
                return Ok();
            } else {
                Response.Headers.Location = $"/{id}";

                return StatusCode((int)HttpStatusCode.Created);
            }
        } catch (JsonReaderException jex) {
            Console.WriteLine("JsonReaderException occurred processing this request");
            Console.WriteLine(jex.Message);
            return BadRequest("Invalid request body");
        } catch (Exception ex) {
            Console.WriteLine("Unknown exception occurred processing this request");
            Console.WriteLine(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred processing this request");
        }
    }
    
    [HttpPatch("{id}")]
    public ActionResult Update(string id) {
        try {
            Object? language;

            using (StreamReader reader = new StreamReader(Request.Body)) {
                language = JsonConvert.DeserializeObject<Object>(reader.ReadToEnd());

                if (language is null)
                {
                    return BadRequest("Invalid request body");
                }
            }

            ResponseCode respCode = _languagesService.UpdateOne(id, language);

            if (respCode == ResponseCode.InvalidId) {
                return BadRequest("The given id is not a valid id");
            } else if (respCode == ResponseCode.NotFound) {
                return NotFound("No language found with that id to update");
            } else {
                return Ok();
            }
        } catch (JsonReaderException jex) {
            Console.WriteLine("JsonReaderException occurred processing this request");
            Console.WriteLine(jex.Message);
            return BadRequest("Invalid request body");
        } catch (Exception ex) {
            Console.WriteLine("Unknown exception occurred processing this request");
            Console.WriteLine(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred processing this request");
        }
    }
    
    [HttpDelete("{id}")]
    public ActionResult Delete(string id) {
        try {
            ResponseCode? respCode = _languagesService.DeleteOne(id);

            if (respCode == ResponseCode.InvalidId) {
                return BadRequest("The given id is not a valid id");
            } else if (respCode == ResponseCode.NotFound) {
                return NotFound("No language found with that id to delete");
            } else {
                return NoContent();
            }
        } catch (JsonReaderException jex) {
            Console.WriteLine("JsonReaderException occurred processing this request");
            Console.WriteLine(jex.Message);
            return BadRequest("Invalid request body");
        } catch (Exception ex) {
            Console.WriteLine("Unknown exception occurred processing this request");
            Console.WriteLine(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred processing this request");
        }
    }
}