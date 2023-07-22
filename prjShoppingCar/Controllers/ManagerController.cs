using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using prjShoppingCar.Models;
using PagedList;
using System.Data;
using System.Data.SqlClient;


namespace prjShoppingCar.Controllers
{
    [Authorize]  //指定Member控制器所有的動作方法必須通過授權才能執行。
    public class ManagerController : Controller
    {
        //建立可存取dbShoppingCar.mdf 資料庫的dbShoppingCarEntities 類別物件db
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        int orderListPageSize = 10;
        int productListPageSize = 10;
        
        //View - 顯示首頁
        [Authorize]
        public ActionResult Index()
        {
            return View("../Manager/Index", "_LayoutManager");
        }
        
        //View - 分頁顯示所有訂單
        [Authorize]
        public ActionResult OrderList(int page = 1)
        {
            //page檢查
            int currentPage = page < 1 ? 1 : page;
            //找出所有會員的訂單記錄並依照fDate進行遞增排序
            var orders = db.tOrder.OrderByDescending(m => m.fDate).ToPagedList(currentPage, orderListPageSize);
            //目前會員的訂單主檔OrderList.cshtml檢視使用orders模型
            return View("../Manager/OrderList", "_LayoutManager", orders);
        }

        //View - 顯示訂單詳細
        public ActionResult OrderDetail(string fOrderGuid)
        {
            //根據fOrderGuid找出和訂單主檔關聯的訂單明細，並指定給orderDetails
            var orderDetails = db.tOrderDetail
                .Where(m => m.fOrderGuid == fOrderGuid).ToList();
            //目前訂單明細的OrderDetail.cshtml檢視使用orderDetails模型
            var order = db.tOrder.Where(m => m.fOrderGuid == fOrderGuid).FirstOrDefault();
            if (order != null) {
                ViewBag.shipping = order.fShipping;
                ViewBag.pickAddress = order.fPickAddress;
                ViewBag.payment = order.fPayment;
            }
            return View(orderDetails);
        }

        //View - 顯示產品頁面
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.Message = TempData["Message"];
            tProduct inputProduct = (tProduct)TempData["Product"];
            if(ViewBag.Message != null) ModelState.AddModelError("fImg", ViewBag.Message);
            return inputProduct == null ? View("../Manager/Create", "_LayoutManager") : View("../Manager/Create", "_LayoutManager", inputProduct);
        }

        //POST - 新增一筆產品並跳轉至產品管理
        [Authorize]
        [HttpPost]
        public ActionResult Create(tProduct product, HttpPostedFileBase productImg)
        {            
            string fileType = System.IO.Path.GetExtension(productImg.FileName);
            string filePath = "~/images/";
            if (String.IsNullOrEmpty(product.fType)) product.fType = "Other";

            filePath = Server.MapPath(filePath);

            if (!String.IsNullOrEmpty(fileType))
            {
                fileType = fileType.ToLower();
                if ("(.png)|(.jpg)|(.jpeg)|(.bmp)".Contains(fileType))
                {
                    db.tProduct.Add(product);
                    db.SaveChanges();
                    
                    string fId = product.fPId;
                    string fImg = fId.ToString() + fileType;
                    productImg.SaveAs(filePath + fImg);

                    product.fImg = fImg;
                    db.SaveChanges();

                    TempData["Message"] = $"商品上架成功(ID:{fId})";
                    return RedirectToAction("ProductManage");
                }
            }
            TempData["Message"] = "檔案格式異常。";
            TempData["Product"] = product;
            return RedirectToAction("Create");    //如果想跳轉回新增頁面就改用這行
        }

        //View - 顯示所有商品清單
        [Authorize]
        public ActionResult ProductManage(string CurrentFilter,string SearchString,int page = 1, string type = "ALL", string order="PId_up")
        {
            if (TempData["Type"] != null) type = TempData["Type"].ToString();
            string currentType = type;
            string currentOrder = order;
            ViewBag.currentType = currentType;
            ViewBag.currentOrder = currentOrder;
            //page檢查
            int currentPage = page < 1 ? 1 : page;
            ViewBag.Message = TempData["Message"];
            TempData["Message"] = "";
            List<tProduct> products = null;

            if (type == "ALL")
            {
                switch (order)
                {
                    case "PId_up":
                        products = db.tProduct.OrderBy(m => m.fId).ToList();
                        break;
                    case "PId_down":
                        products = db.tProduct.OrderByDescending(m => m.fId).ToList();
                        break;
                    case "Sold_up":
                        products = db.tProduct.OrderBy(m => m.fSoldCount).ThenBy(m => m.fId).ToList();
                        break;
                    case "Sold_down":
                        products = db.tProduct.OrderByDescending(m => m.fSoldCount).ThenBy(m => m.fId).ToList();
                        break;
                    default:
                        products = db.tProduct.OrderBy(m => m.fId).ToList();
                        break;
                }
            }
            else {
                switch (order)
                {
                    case "PId_up":
                        products = db.tProduct.Where(m => m.fType == currentType).OrderBy(m => m.fId).ToList();
                        break;
                    case "PId_down":
                        products = db.tProduct.Where(m => m.fType == currentType).OrderByDescending(m => m.fId).ToList();
                        break;
                    case "Sold_up":
                        products = db.tProduct.Where(m => m.fType == currentType).OrderBy(m => m.fSoldCount).ThenBy(m => m.fId).ToList();
                        break;
                    case "Sold_down":
                        products = db.tProduct.Where(m => m.fType == currentType).OrderByDescending(m => m.fSoldCount).ThenBy(m => m.fId).ToList();
                        break;
                    default:
                        products = db.tProduct.Where(m => m.fType == currentType).OrderBy(m => m.fId).ToList();
                        break;
                }
            }
            ViewBag.currentCount = products.Count;
            var result = products.ToPagedList(currentPage, orderListPageSize);
            if (SearchString != null)
            {
                page = 1;
            }
            else
            {
                SearchString = CurrentFilter;
            }
            ViewBag.CurrentFilter = SearchString;
            if (!String.IsNullOrEmpty(SearchString))
            {
                var filterProducts = db.tProduct.Where(m => m.fName.Contains(SearchString)).OrderBy(m => m.fId).ToList();
                int currentPage1 = page < 1 ? 1 : page;
                result = filterProducts.ToPagedList(currentPage1,orderListPageSize);
                return View("../Manager/ProductManage", "_LayoutManager", result);
            };


            return View("../Manager/ProductManage", "_LayoutManager", result);
        }

        //View - 顯示商品編輯頁面
        [Authorize]
        public ActionResult Edit(int id, string type = "ALL")
        {
            var product = db.tProduct.Where(m => m.fId == id).FirstOrDefault();
            if(product == null)
            {
                TempData["Message"] = $"欲編輯商品(ID:{id})異常。";
                TempData["Type"] = type;
                return RedirectToAction("ProductManage");
            }
            ViewBag.currentType = type;
            return View("../Manager/Edit", "_LayoutManager", product);
        }

        //POST - 儲存商品編輯資料
        [Authorize]
        [HttpPost]
        public ActionResult Edit(tProduct product, HttpPostedFileBase productImg, string fPId, string type = "ALL")
        {
            var dbProduct = db.tProduct.Where(m => m.fPId == fPId).FirstOrDefault();
            TempData["Type"] = type;
            if (dbProduct == null)
            {
                TempData["Message"] = $"欲編輯商品(ID:{fPId})不存在。";
                return RedirectToAction("ProductManage");
            }

            dbProduct.fPId = product.fPId;
            dbProduct.fName = product.fName;
            dbProduct.fPrice = product.fPrice;
            dbProduct.fintroduce = product.fintroduce;
            dbProduct.fType = product.fType;

            if(productImg != null)
            {            
                string fileType = System.IO.Path.GetExtension(productImg.FileName);
                string filePath = "~/images/";

                filePath = Server.MapPath(filePath);

                if (!String.IsNullOrEmpty(fileType))
                {
                    fileType = fileType.ToLower();
                    if ("(.png)|(.jpg)|(.jpeg)|(.bmp)".Contains(fileType))
                    {
                        string fImg = fPId.ToString() + fileType;
                        productImg.SaveAs(filePath + fImg);
                        product.fImg = fImg;
                    }
                }
            }

            dbProduct.fImg = product.fImg;
            db.SaveChanges();

            TempData["Message"] = $"{fPId} 編輯成功。";
            return RedirectToAction("ProductManage");
        }

        //Delete - 刪除商品資料
        [Authorize]
        public ActionResult Delete(int? id, string type = "ALL")
        {
            var product = db.tProduct.Where(m => m.fId == id).FirstOrDefault();
            TempData["Type"] = type;
            if (product == null)
            {
                TempData["Message"] = $"欲刪除商品(ID:{id})不存在。";                
                return RedirectToAction("ProductManage");
            }
            db.tProduct.Remove(product);
            db.SaveChanges();
            TempData["Message"] = $"{id} 刪除成功。";
            return RedirectToAction("ProductManage");
        }
        
    }
}