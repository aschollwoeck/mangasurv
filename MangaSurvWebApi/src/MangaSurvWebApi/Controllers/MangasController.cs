﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MangaSurvWebApi.Model;
using Microsoft.AspNetCore.Authorization;
using MangaSurvWebApi.Service;

namespace MangaSurvWebApi.Controllers
{
    [Route("api/[controller]")]
    public class MangasController : Controller
    {
        private readonly MangaSurvContext _context;
        public MangasController(MangaSurvContext context)
        {
            this._context = context;
        }

        // GET api/mangas
        [HttpGet]
        public IActionResult Get()
        {
            if (Request.QueryString.HasValue)
            {
                Helper.QueryString queryString = new Helper.QueryString(Request);
                if (queryString.ContainsKey("CHAPTERSTATEID"))
                {
                    List<Chapter> lNewChapters = this._context.Chapters.Where(c => c.StateId == int.Parse(queryString.GetValue("CHAPTERSTATEID"))).ToList();
                    List<Manga> lMangas = new List<Manga>();
                    List<long> lMangaIds = new List<long>();

                    foreach (Chapter chapter in lNewChapters)
                    {
                        if (lMangaIds.Contains(chapter.MangaId))
                        {
                            Manga manga = lMangas.Find(m => m.Id == chapter.MangaId);
                            manga.Chapters.Add(chapter);
                        }
                        else
                        {
                            Manga manga = this._context.Mangas.FirstOrDefault(m => m.Id == chapter.MangaId);
                            if (manga != null)
                            {
                                lMangas.Add(manga);
                                lMangaIds.Add(manga.Id);
                            }
                        }
                    }

                    return this.Ok(lMangas.OrderBy(manga => manga.Name));
                }
                else if (queryString.ContainsKey("INCLUDE"))
                {
                    return this.Ok(this._context.Mangas.Include(m => m.Chapters.OrderBy(c => c.ChapterNo)).OrderBy(manga => manga.Name));
                }
                else if(queryString.ContainsKey("NAME"))
                {
                    return this.Ok(this._context.Mangas.Where(m => m.Name == queryString.GetValue("NAME")));
                }
            }

            var mangas = this._context.Mangas.OrderBy(manga => manga.Name);
            return this.Ok(mangas);
        }

        // GET api/mangas/5
        [HttpGet("{id}", Name ="MangaLink")]
        [Produces(typeof(Manga))]
        public IActionResult Get(int id)
        {
            Helper.QueryString queryString = new Helper.QueryString(Request);
            if(queryString.ContainsKeys())
            {
                if (queryString.ContainsKey("INCLUDE"))
                {
                    var result = this._context.Mangas.Where(m => m.Id == id).Include(m => m.Chapters).FirstOrDefault();
                    if (result == null)
                        return this.NotFound();

                    result.Chapters = result.Chapters.OrderByDescending(c => c.ChapterNo).ToList();
                    return this.Ok(result);
                }
            }

            Manga manga = this._context.Mangas.FirstOrDefault(m => m.Id == id);
            if (manga == null)
                return this.NotFound();

            return this.Ok(manga);
        }

        // POST api/mangas
        [Authorize(Roles = WebApiAccess.WRITE_ROLE)]
        [HttpPost]
        public IActionResult Post([FromBody]Manga value)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(ModelState);

                Manga.AddManga(value);

                return this.CreatedAtRoute("MangaLink", new { id = value.Id }, value);
            }
            catch(Exception ex)
            {
                return this.BadRequest(ex);
            }
        }

        // PUT api/mangas/5
        [Authorize(Roles = WebApiAccess.WRITE_ROLE)]
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]Manga value)
        {
            this._context.Mangas.Attach(value);

            var manga = this._context.Mangas.FirstOrDefault(m => m.Id == id);

            this._context.Entry(manga).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            this._context.SaveChanges();
            this.Ok(manga);
        }

        // DELETE api/mangas/5
        [Authorize(Roles = WebApiAccess.WRITE_ROLE)]
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            Manga manga = this._context.Mangas.FirstOrDefault(m => m.Id == id);
            if (manga != null)
            { 
                this._context.Mangas.Remove(manga);
                this._context.SaveChanges();
            }
        }
    }
}
