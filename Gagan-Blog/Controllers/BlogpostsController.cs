using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Gagan_Blog.Helpers;
using Gagan_Blog.Models;
using PagedList;
using Microsoft.AspNet.Identity;

namespace Gagan_Blog.Controllers
{
    [RequireHttps]
    public class BlogpostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private string slug;

        // GET: Blogposts
        public ActionResult Index(int? page, string searchString)
        {
            int pageSize = 1; // display three blog posts at a time on this page
            int pageNumber = (page ?? 1);

            var postQuery = db.Posts.OrderBy(p => p.Created).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                postQuery = postQuery
                    .Where(p => p.Title.Contains(searchString) ||
                                p.Body.Contains(searchString) ||
                                p.Slug.Contains(searchString) ||
                                p.Comments.Any(t => t.Body.Contains(searchString))
                           ).AsQueryable();
            }


            var postList = postQuery.ToPagedList(pageNumber, pageSize);
            return View(postList);
        }

        // GET: BlogPosts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blogpost blogpost = db.Posts.Find(id);
            if (blogpost == null)
            {
                return HttpNotFound();
            }
            return View(blogpost);
        }

        // GET: Blogposts/Details/5
        public ActionResult DetailsSlug(string slug)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blogpost blogpost = db.Posts
                .Include(p => p.Comments.Select(t => t.Author))
                .Where(p => p.Slug == slug)
                .FirstOrDefault();
            if (blogpost == null)
            {
                return HttpNotFound();
            }
            return View("Details", blogpost);
        }

                // POST: BlogPosts/Details/5
        [HttpPost]
        public ActionResult DetailsSlug(string slug, string body)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
             var blogPost = db.Posts
                .Where(p => p.Slug == slug)
                .FirstOrDefault();
             if (blogPost == null)
            {
                return HttpNotFound();
            }
             if (string.IsNullOrWhiteSpace(body))
            {
                ViewBag.ErrorMessage = "Comment is required";
                return View("Details", blogPost);
            }
             var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.BlogPostId = blogPost.Id;
            comment.Created = DateTime.Now;
            comment.Body = body;
             db.Comments.Add(comment);
            db.SaveChanges();
             return RedirectToAction("DetailsSlug", new { slug = slug });
        }


        [HttpPost]
        public ActionResult BlogpostController(string slug, string body)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var blogPost = db.Posts
                .Where(p => p.Slug == slug)
                .FirstOrDefault();

            if (blogPost == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                ViewBag.ErrorMessage = "Comment is required";
                return View("Details", blogPost);
            }

            var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.BlogPostId = blogPost.Id;
            comment.Created = DateTime.Now;
            comment.Body = body;

            db.Comments.Add(comment);
            db.SaveChanges();

            return RedirectToAction("DetailsSlug", new { slug = slug });
        }

        // GET: Blogposts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Blogposts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Created,Updated,Title,Slug,Body,MediaUrl,Published")] Blogpost blogpost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var Slug = StringUtilites.URLFriendly(blogpost.Title);
                if (String.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid title");
                    return View(blogpost);
                }

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogpost.MediaUrl = "/Uploads/" + fileName;
                }

                if (db.Posts.Any(p => p.Slug == Slug))
                {
                    ModelState.AddModelError("Title", "The title must be unique");
                    return View(blogpost);
                }

                blogpost.Slug = Slug;
                blogpost.Created = DateTimeOffset.Now;
                db.Posts.Add(blogpost);
                db.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(blogpost);
        }

        // GET: Blogposts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blogpost blogpost = db.Posts.Find(id);
            if (blogpost == null)
            {
                return HttpNotFound();
            }
           
            return View(blogpost);
        }

        // POST: Blogposts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Slug,Body,MediaUrl,Published")] Blogpost blogpost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogpost.MediaUrl = "/Uploads/" + fileName;
                }

                db.Entry(blogpost).State = EntityState.Modified;

                blogpost.Updated = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            };
           

            return View(blogpost);
        }


        // GET: Blogposts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blogpost blogpost = db.Posts.Find(id);
            if (blogpost == null)
            {
                return HttpNotFound();
            }
            return View(blogpost);
        }

        // POST: Blogposts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Blogpost blogpost = db.Posts.Find(id);
            db.Posts.Remove(blogpost);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize]
        public ActionResult CreateComment(string slug, string body)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var blogPost = db.Posts
               .Where(p => p.Slug == slug)
               .FirstOrDefault();
            if (blogPost == null)
            {
                return HttpNotFound();
            }


            if (string.IsNullOrWhiteSpace(body))
            {
                TempData["ErrorMessage"] = "Comment is required";
                return RedirectToAction("DetailsSlug", new { slug = slug });
            }


            var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.BlogPostId = blogPost.Id;
            comment.Created = DateTime.Now;
            comment.Body = body;
            db.Comments.Add(comment);
            db.SaveChanges();
            return RedirectToAction("DetailsSlug", new { slug = slug });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
