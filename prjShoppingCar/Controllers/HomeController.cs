
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using prjShoppingCar.Models;
using PagedList;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EllipticCurve.Utils;
using String = System.String;

namespace prjShoppingCar.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //建立可存取dbShoppingCar.mdf 資料庫的dbShoppingCarEntities 類別物件db
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        int pageSize = 6;
        // GET: Home/Index

        [Authorize]
        public ActionResult Index(string HpriceFilter, string LpriceFilter, string CurrentFilter, string LowestPrice, string HighestPrice, string SearchString, int page = 1)
        {

            //取得所有產品放入products
            
            int currentPage = page < 1 ? 1 : page;
            var products = db.tProduct
                .OrderByDescending(m => m.fId).ToList();
            ViewBag.currentCount = products.Count;
            var result = products.ToPagedList(currentPage, pageSize);
            if (SearchString != null || HighestPrice != null || LowestPrice != null)
            {
                if (!String.IsNullOrEmpty(SearchString) & !String.IsNullOrEmpty(HighestPrice) & !String.IsNullOrEmpty(LowestPrice))
                {
                    int high = Int32.Parse(HighestPrice);
                    int low = Int32.Parse(LowestPrice);
                    if (high < low)
                    {
                        ViewBag.Message1 = "錯誤的查詢條件。";
                        CurrentFilter = SearchString;
                        HpriceFilter = HighestPrice;
                        LpriceFilter = LowestPrice;
                    }
                    CurrentFilter = SearchString;
                    HpriceFilter = HighestPrice;
                    LpriceFilter = LowestPrice;
                }
                else if(!String.IsNullOrEmpty(HighestPrice) &String.IsNullOrEmpty(LowestPrice) & !String.IsNullOrEmpty(SearchString))
                {   
                        HpriceFilter = HighestPrice;
                        CurrentFilter = SearchString;
                        LowestPrice = LpriceFilter;  
                }
                else if (!String.IsNullOrEmpty(LowestPrice) &String.IsNullOrEmpty(HighestPrice) & !String.IsNullOrEmpty(SearchString))
                {
                    HighestPrice = HpriceFilter;
                    CurrentFilter = SearchString;
                    LpriceFilter = LowestPrice;
                }
                else if (String.IsNullOrEmpty(HighestPrice) & String.IsNullOrEmpty(LowestPrice) & !String.IsNullOrEmpty(SearchString))
                {
                    CurrentFilter = SearchString;
                    HighestPrice = HpriceFilter;
                    LowestPrice = LpriceFilter;
                }
                else if (String.IsNullOrEmpty(SearchString) & !String.IsNullOrEmpty(HighestPrice) & !String.IsNullOrEmpty(LowestPrice))
                {
                    int high = Int32.Parse(HighestPrice);
                    int low = Int32.Parse(LowestPrice);
                    if (high < low)
                    {
                        ViewBag.Message1 = "錯誤的查詢條件。";
                        SearchString = CurrentFilter;
                        HpriceFilter = HighestPrice;
                        LpriceFilter = LowestPrice;
                    }
                    SearchString = CurrentFilter;
                    HpriceFilter = HighestPrice;
                    LpriceFilter = LowestPrice;
                }
                else if(String.IsNullOrEmpty(SearchString)& String.IsNullOrEmpty(HighestPrice)& !String.IsNullOrEmpty(LowestPrice))
                {
                    HighestPrice = HpriceFilter;
                    LpriceFilter = LowestPrice;
                    SearchString = CurrentFilter;
                }
                else if(String.IsNullOrEmpty(SearchString) & !String.IsNullOrEmpty(HighestPrice) & String.IsNullOrEmpty(LowestPrice))
                {
                    HpriceFilter = HighestPrice;
                    LowestPrice = LpriceFilter;
                    SearchString = CurrentFilter;
                }
                    
            }
            else
            {
                SearchString = CurrentFilter;
                HighestPrice = HpriceFilter;
                LowestPrice = LpriceFilter;
            }
            ViewBag.CurrentFilter = SearchString;
            ViewBag.HpriceFilter = HighestPrice;
            ViewBag.LpriceFilter = LowestPrice;
                
                
           
            

            if (!String.IsNullOrEmpty(SearchString)&!String.IsNullOrEmpty(HighestPrice)&!String.IsNullOrEmpty(LowestPrice))
            {
                int high = Int32.Parse(HighestPrice);
                int low = Int32.Parse(LowestPrice);
                var g = from m in db.tProduct.OrderBy(m => m.fId) select m;
                g = g.Where(c => c.fName.Contains(SearchString)&&c.fPrice<high&c.fPrice>low);

                int currentPage1 = page < 1 ? 1 : page;
                result = g.ToPagedList(currentPage1, pageSize);
                return View("../Home/Index", "_LayoutMember", result);
               
            }
            if(!String.IsNullOrEmpty(HighestPrice) & String.IsNullOrEmpty(LowestPrice) & !String.IsNullOrEmpty(SearchString))
            {
                int high = Int32.Parse(HighestPrice);
                var g = from m in db.tProduct.OrderBy(m => m.fId) select m;
                g = g.Where(c => c.fName.Contains(SearchString) && c.fPrice < high);
                int currentPage1 = page < 1 ? 1 : page;
                result = g.ToPagedList(currentPage1, pageSize);
                return View("../Home/Index", "_LayoutMember", result);
            }
            if (!String.IsNullOrEmpty(LowestPrice) & string.IsNullOrEmpty(HighestPrice) & !string.IsNullOrEmpty(SearchString))
            {
                int low = Int32.Parse(LowestPrice);
                var g = from m in db.tProduct.OrderBy(m => m.fId) select m;
                g = g.Where(c => c.fName.Contains(SearchString) && c.fPrice > low);
                int currentPage1 = page < 1 ? 1 : page;
                result = g.ToPagedList(currentPage1, pageSize);
                return View("../Home/Index", "_LayoutMember", result);
            }
            if (String.IsNullOrEmpty(HighestPrice) & String.IsNullOrEmpty(LowestPrice) & !String.IsNullOrEmpty(SearchString))
            {

                var g = from m in db.tProduct.OrderBy(m => m.fId) select m;
                g = g.Where(c => c.fName.Contains(SearchString));

                int currentPage1 = page < 1 ? 1 : page;
                result = g.ToPagedList(currentPage1, pageSize);
                return View("../Home/Index", "_LayoutMember", result);
            }
            if (String.IsNullOrEmpty(SearchString) & !String.IsNullOrEmpty(HighestPrice) & !String.IsNullOrEmpty(LowestPrice))
            {
                int high = Int32.Parse(HighestPrice);
                int low = Int32.Parse(LowestPrice);
                var g = from m in db.tProduct.OrderBy(m => m.fId) select m;
                g = g.Where(c => c.fPrice < high & c.fPrice > low);
                int currentPage1 = page < 1 ? 1 : page;
                result = g.ToPagedList(currentPage1, pageSize);
                return View("../Home/Index", "_LayoutMember", result);
            }
            if (String.IsNullOrEmpty(SearchString) & !String.IsNullOrEmpty(HighestPrice) & String.IsNullOrEmpty(LowestPrice))
            {
                int high = Int32.Parse(HighestPrice);
                var g = from m in db.tProduct.OrderBy(m => m.fId) select m;
                g = g.Where(c => c.fPrice < high );
                int currentPage1 = page < 1 ? 1 : page;
                result = g.ToPagedList(currentPage1, pageSize);
                return View("../Home/Index", "_LayoutMember", result);
            }
            if (String.IsNullOrEmpty(SearchString) & String.IsNullOrEmpty(HighestPrice) & !String.IsNullOrEmpty(LowestPrice))
            {
                int low = Int32.Parse(LowestPrice);
                var g = from m in db.tProduct.OrderBy(m => m.fId) select m;
                g = g.Where(c => c.fPrice > low);
                int currentPage1 = page < 1 ? 1 : page;
                result = g.ToPagedList(currentPage1, pageSize);
                return View("../Home/Index", "_LayoutMember", result);
            }


            return View(result);

        }
        [AllowAnonymous]
        public ActionResult Create()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Create
            (string fPId, string fName, int fPrice, string fImg, string fintroduce, HttpPostedFileBase photo)
        {

            tProduct todo = new tProduct();
            todo.fPId = fPId;
            todo.fName = fName;
            todo.fPrice = fPrice;
            todo.fImg = fImg;
            todo.fintroduce = fintroduce;
            db.tProduct.Add(todo);
            db.SaveChanges();
            return RedirectToAction("Login");
        }
        [AllowAnonymous]
        //Get: Home/Login
        public ActionResult Login()
        {
            return View();
        }
        //Post: Home/Login
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string fUserId, string fPwd)
        {
            // 依帳密取得會員並指定給member
            var member = db.tMember
                .Where(m => m.fUserId == fUserId && m.fPwd == fPwd)
                .FirstOrDefault();
            //若member為null，表示會員未註冊
            if (member == null)
            {
                ViewBag.Message = "帳號或密碼錯誤，請重新輸入。";
                return View();
            }
            //管理員
            if (member.fPower == "root")
            {
                Session["WelCome"] = member.fName + "歡迎光臨";
                FormsAuthentication.RedirectFromLoginPage(fUserId, true);
                return RedirectToAction("Index", "Manager");
            }
            //使用Session變數記錄歡迎詞
            Session["WelCome"] = member.fName + "歡迎光臨";
            FormsAuthentication.RedirectFromLoginPage(fUserId, true);
            return RedirectToAction("Index", "Member");
        }
        [AllowAnonymous]
        //Get:Home/Register
        public ActionResult Register()
        {
            return View();
        }
        //Post:Home/Register
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Register(tMember pMember)
        {
            //若模型沒有通過驗證則顯示目前的View
            if (ModelState.IsValid == false)
            {
                return View();
            }
            // 依帳號取得會員並指定給member
            var member = db.tMember
                .Where(m => m.fUserId == pMember.fUserId)
                .FirstOrDefault();
            //若member為null，表示會員未註冊
            if (member == null)
            {
                if (String.IsNullOrEmpty(pMember.fPower)) pMember.fPower = "normal";
                //將會員記錄新增到tMember資料表
                db.tMember.Add(pMember);
                db.SaveChanges();
                //執行Home控制器的Login動作方法
                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號己有人使用，註冊失敗";
            return View();
        }
        
        
        
    }
}