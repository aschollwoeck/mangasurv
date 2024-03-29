﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MangaSurvWebApi.Model;
using Microsoft.AspNetCore.Authorization;
using MangaSurvWebApi.Service;

namespace MangaSurvWebApi.Controllers
{
    [Route("api/users")]
    public class UsersChaptersController : Controller
    {
        private readonly MangaSurvContext _context;
        public UsersChaptersController(MangaSurvContext context)
        {
            this._context = context;
        }

        // GET api/chapters
        [Authorize(Roles = WebApiAccess.USER_ROLE)]
        [HttpGet("{userid}/chapters/")]
        public IActionResult Get(int userid)
        {
            UserTokenDetails userDetails = new UserTokenDetails(User);
            User user = Model.User.GetUser(userid, userDetails);

            if (user == null)
                return this.Forbid();

            if (Request.QueryString.HasValue)
            {
                Helper.QueryString queryString = new Helper.QueryString(Request);
                if (queryString.ContainsKey("SORTBY") && queryString.GetValue("SORTBY").ToUpper().Equals("MANGA"))
                {
                    List<Chapter> lNewChapters = (from chapter in _context.UserNewChapters
                                                  where chapter.UserId == user.Id
                                                  orderby chapter.Chapter
                                                  select chapter.Chapter).ToList();

                    return this.Ok(lNewChapters.SortByManga().OrderBy(m => m.Name));
                }
            }

            var chapterslist = (from chapter in _context.UserNewChapters
                                where chapter.UserId == user.Id
                                select chapter.Chapter).ToList();

            return this.Ok(chapterslist);
        }

        // GET api/chapters/5
        [Authorize(Roles = WebApiAccess.USER_ROLE)]
        [HttpGet("{userid}/chapters/{chapterid}", Name = "UserChapterLink")]
        [Produces(typeof(Chapter))]
        public IActionResult Get(int userid, int chapterid)
        {
            UserTokenDetails userDetails = new UserTokenDetails(User);
            User user = Model.User.GetUser(userid, userDetails);

            if (user == null)
                return this.Forbid();

            List<Chapter> chapterslist = (from chapter in _context.UserNewChapters
                                          where chapter.UserId == user.Id
                                          && chapter.ChapterId == chapterid
                                          select chapter.Chapter).ToList();

            return this.Ok(chapterslist.FirstOrDefault());
        }

        // POST api/chapters
        [Authorize(Roles = WebApiAccess.USER_ROLE)]
        [HttpPost("{userid}/chapters/")]
        public async Task<IActionResult> Post(int userid, [FromBody]Chapter value)
        {
            try
            {
                UserTokenDetails userDetails = new UserTokenDetails(User);
                User user = Model.User.GetUser(userid, userDetails);

                if (user == null)
                    return this.Forbid();

                var chapter = this._context.Chapters.FirstOrDefault(m => m.Id == value.Id);
                if (chapter == null)
                    return this.NotFound();

                var entry = await UserNewChapters.AddChapterToUser(chapter, user.Id);

                return this.CreatedAtRoute("UserChapterLink", new { userid = entry.UserId, chapterid = entry.ChapterId }, entry);
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex);
            }
        }

        // PUT api/chapters/5
        //[Authorize(Roles = WebApiAccess.USER_ROLE)]
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]Chapter value)
        //{
        //}

        // DELETE api/chapters/5
        [Authorize(Roles = WebApiAccess.USER_ROLE)]
        [HttpDelete("{userid}/chapters/{chapterid}")]
        public IActionResult Delete(int userid, int chapterid)
        {
            try
            {
                UserTokenDetails userDetails = new UserTokenDetails(User);
                User user = Model.User.GetUser(userid, userDetails);

                if (user == null)
                    return this.Forbid();

                var userchapters = this._context.UserNewChapters.Where(u => u.UserId == user.Id && u.ChapterId == chapterid);
                if (userchapters == null || userchapters.Count() == 0)
                    return this.Ok();

                this._context.UserNewChapters.RemoveRange(userchapters);
                this._context.SaveChangesAsync();
            }
            catch
            {

            }

            return this.Ok();
        }

        // DELETE api/chapters/5
        [Authorize(Roles = WebApiAccess.USER_ROLE)]
        [HttpDelete("{userid}/chapters")]
        public IActionResult DeleteAllChapters(int userid)
        {
            try
            {
                if (Request.QueryString.HasValue)
                {
                    Helper.QueryString queryString = new Helper.QueryString(Request);
                    if (queryString.ContainsKey("MANGAID") && !String.IsNullOrEmpty(queryString.GetValue("MANGAID")))
                    {

                        UserTokenDetails userDetails = new UserTokenDetails(User);
                        User user = Model.User.GetUser(userid, userDetails);

                        if (user == null)
                            return this.Forbid();

                        int mangaId = int.Parse(queryString.GetValue("MANGAID"));
                        var userchapters = (from chapter in _context.UserNewChapters
                                                      where chapter.UserId == user.Id
                                                      && chapter.Chapter.MangaId == mangaId
                                                      select chapter).ToList();

                        if (userchapters != null && userchapters.Count() > 0)
                        {
                            this._context.UserNewChapters.RemoveRange(userchapters);
                            this._context.SaveChangesAsync();
                        }

                        return this.Ok();
                    }
                }
            }
            catch
            {

            }

            return this.BadRequest();
        }

        [HttpOptions]
        public IActionResult Options()
        {
            return this.Ok();
        }
    }
}
