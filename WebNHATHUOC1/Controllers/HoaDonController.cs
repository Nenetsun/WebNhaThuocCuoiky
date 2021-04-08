using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNHATHUOC1.Models;
using WebNHATHUOC1.Controllers;

namespace WebNHATHUOC1.Controllers
{
    public class HoaDonController : Controller
    {
        QLNT db = new QLNT();
        public ActionResult Index()
        {
            return View(db.HOADONs.ToList());
        }
        #region Lập đơn thuốc

        public ActionResult FormDonThuoc()
        {
            ViewBag.DSThuoc = db.THUOCs.ToList();
            ViewBag.DSCTHD = Session["DSCTHD"] as List<Models.CHITIETHOADON>;
            string sdt = Session["Sodt"] as string;
            ViewBag.KH = db.KHACHHANGs.Find(sdt);
            return View();
        }
        [HttpGet]
        public ActionResult chonThuoc(string name, string sohd)
        {
            if(Session["Mahd"]==null || Session["Mahd"] as string == "") Session["Mahd"] = sohd;

            List<Models.THUOC> ds = db.THUOCs.Where(x => 
                (x.tenthuoc.ToLower().Contains(name.ToLower().Trim())
                || x.mathuoc.ToLower().Contains(name.ToLower().Trim())
                || x.mota.ToLower().Contains(name.ToLower().Trim())
                || x.donvitinh.ToLower().Contains(name.ToLower().Trim()))
                && x.hethan == false
            ).ToList();

            if (ds.Count <= 0)
                ds = db.THUOCs.Where(x => x.hethan == false).ToList();

            return PartialView(ds);
        }
        [HttpGet]
        public ActionResult addThuoc(string mathuoc)
        {
            if(Session["Mahd"] as string == "" || Session["Mahd"]==null)
            {
                ModelState.AddModelError("sohd", "Chưa nhập mã hóa đơn!");
                return View("FormDonThuoc");
            }
            else if(db.HOADONs.Find(Session["Mahd"] as string) != null)
            {
                ModelState.AddModelError("sohd", "Số hóa đơn bị trùng!");
                Session["Mahd"] = null;
                return View("FormDonThuoc");
            }
            else
            {
                if (Session["DSCTHD"] == null) Session["DSCTHD"] = new List<Models.CHITIETHOADON>();
                List<Models.CHITIETHOADON> ds = Session["DSCTHD"] as List<Models.CHITIETHOADON>;
                if(ds.Where(x=>x.mathuoc == mathuoc).ToList().Count != 0)
                {
                    return View("FormDonThuoc");
                }
                else
                {
                    CHITIETHOADON cthd = new CHITIETHOADON();
                    cthd.sohd = Session["Mahd"] as string;
                    cthd.THUOC = db.THUOCs.Find(mathuoc);
                    cthd.mathuoc = cthd.THUOC.mathuoc;
                    cthd.dongia = cthd.THUOC.dongia;
                    cthd.donvitinh = cthd.THUOC.donvitinh;
                    cthd.soluong = 1;
                    cthd.thanhtien = cthd.soluong * cthd.dongia;
                    ds.Add(cthd);
                }
                Session["DSCTHD"] = ds;
            }    
            return RedirectToAction("FormDonThuoc");
        }
        [HttpGet]
        public ActionResult updateCTHD(int index, byte soluong)
        {
            List<Models.CHITIETHOADON> ds = Session["DSCTHD"] as List<Models.CHITIETHOADON>;
            CHITIETHOADON cthd = ds[index];
            cthd.soluong = soluong;
            cthd.thanhtien = cthd.soluong * cthd.dongia;
            if (soluong <= 0)
                ds.RemoveAt(index);
            Session["DSCTHD"] = ds;
            return RedirectToAction("FormDonThuoc");
        }
        [HttpGet]
        public ActionResult delCTHD(string mathuoc)
        {
            List<Models.CHITIETHOADON> ds = Session["DSCTHD"] as List<Models.CHITIETHOADON>;
            CHITIETHOADON cthd = ds.Where(x => x.mathuoc == mathuoc).ToList()[0];
            if(cthd != null)
                ds.Remove(cthd);
            if (ds.Count <= 0)
                Session["DSCTHD"] = null;
            return RedirectToAction("FormDonThuoc");
        }
        [HttpGet]
        public ActionResult searchKH(string name, string sohd)
        {
            if (Session["Mahd"] == null) Session["Mahd"] = sohd;

            List<Models.KHACHHANG> ds = db.KHACHHANGs.Where(x => 
                x.tenkh.ToLower().Contains(name.ToLower().Trim()) 
                || x.sodt.ToLower().Contains(name.ToLower().Trim())
            ).ToList();

            if (ds.Count <= 0)
                ds = db.KHACHHANGs.ToList();

            return PartialView(ds);
        }
        public ActionResult getKH(string sodt)
        {
            Session["Sodt"] = sodt;
            return RedirectToAction("FormDonThuoc");
        }
        [HttpPost]
        public ActionResult addHoaDon(string mahd, string sodt)
        {
            if(Session["DSCTHD"] == null)
            {
                ModelState.AddModelError("nameThuoc", "Hóa đơn rỗng!");
                return View("FormDonThuoc");
            }
            else if(Session["Sodt"] == null)
            {
                ModelState.AddModelError("nameKH", "Chưa nhập khách hàng!");
                return View("FormDonThuoc");
            }    
            if(ModelState.IsValid)
            {
                HOADON hd = new HOADON();
                hd.sohd = Session["Mahd"] as string;
                hd.makh = Session["Sodt"] as string;
                hd.ngaylap = System.DateTime.Today;
                NHANVIEN nv = Session["User"] as NHANVIEN;
                hd.manv = nv.manv;
                hd.thanhtien = 0;
                List<CHITIETHOADON> ds = Session["DSCTHD"] as List<CHITIETHOADON>;
                if(ds.Count > 0)
                {
                    foreach(var item in ds)
                    {
                        CHITIETHOADON temp = new CHITIETHOADON();
                        temp.sohd = item.sohd;
                        temp.mathuoc = item.mathuoc;
                        temp.soluong = item.soluong;
                        temp.dongia = item.dongia;
                        temp.donvitinh = item.donvitinh;
                        temp.thanhtien = temp.dongia * temp.soluong;
                        db.CHITIETHOADONs.Add(temp);
                        hd.thanhtien += temp.thanhtien;
                    }
                    db.HOADONs.Add(hd);
                    db.SaveChanges();
                }
            }
            Session["Mahd"] = null;
            Session["Sodt"] = null;
            Session["DSCTHD"] = null;

            return RedirectToAction("Index");
        }

        #endregion

        #region Quản lý hóa đơn

        public ActionResult detailHD(string sohd)
        {
            List<CHITIETHOADON> dscthd = db.CHITIETHOADONs.Where(x => x.sohd == sohd).ToList();
            ViewBag.DSCTHD = dscthd;
            return View(db.HOADONs.Find(sohd));
        }

        #endregion
    }
}