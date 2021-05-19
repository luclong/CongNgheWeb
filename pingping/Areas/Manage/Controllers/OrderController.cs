﻿using Modal.DAO;
using Modal.EF;
using OfficeOpenXml;
using pingping.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace pingping.Areas.Manage.Controllers
{
    public class OrderController : Controller
    {
        // GET: Manage/Order
        public ActionResult QuanLyHoaDon()
        {
            var session_acc = SessionHelper.GetSession();
            if (session_acc == null)
            {
                return RedirectToAction("../../Accounts/Login");
            }

            ViewBag.Name = session_acc.hoten;
            ViewBag.Messager = "Đăng Xuất";
            ViewBag.Login = "../Accounts/Logout";

            var dao = new HoaDon_DAO();
            var lst = dao.get_hoadon_damua_all();
            return View(lst);
        }
        public ActionResult HoaDonChiTiet(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var dao = new HoaDon_DAO();
            var res = dao.get_hoadon_idd(id);
            if (res == null)
            {
                return HttpNotFound();
            }
            var dao1 = new HoaDonCT_DAO();
            var res1 = dao1.get_hdct_hoadon(id);
            ViewBag.ListHoaDonChiTiet = res1;

            var dao_namesize = new Size_DAO();
            //object model = new object();
            List<Size> model_total = new List<Size>();
            object model = new object();
            foreach (var m in res1)
            {
                var namesize = dao_namesize.get_size_id(m.id_size); //get name size
                model_total.Add(namesize);
            }
            ViewBag.lstSize = model_total;
            return View(res);
        }
        public void DownloadExcel()
        {


            var dao = new HoaDon_DAO();
            var lst = dao.get_hoadon_damua_all();
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "No";
            Sheet.Cells["B1"].Value = "Name";
            Sheet.Cells["C1"].Value = "Number Phone";
            Sheet.Cells["D1"].Value = "Order Date";
            Sheet.Cells["E1"].Value = "Total Amount";
            int row = 2;
            foreach (var item in lst)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = row-1;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.NguoiMua.TaiKhoan.hoten;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.NguoiMua.phone;
                Sheet.Cells[string.Format("D{0}", row)].Value = String.Format("{0:d/M/yyyy HH:mm:ss}", item.thoigian);
                var info = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
                Sheet.Cells[string.Format("E{0}", row)].Value = String.Format(info, "{0:c}", item.tonggia);
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        #region DuyetHD
        public JsonResult LoadHoaDonDuyet(int? id)
        {
            if (id != null)
            {
                var dao_ghn = new GHN_DAO();
                var res_ghn = dao_ghn.get_dhn_hoadon_id(id);
                if (res_ghn != null)
                {
                    return Json(res_ghn, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadNguoiNhan(int id)
        {
            if (id <= 0)
            {
                var dao_hd = new HoaDon_DAO();
                var res_ghn = dao_hd.get_hoadon_id(id);

                var dao_ngm = new NguoiMua_DAO();
                var res_ngm = dao_ngm.get_nguoimua_id(id);
                if (res_ngm != null)
                {
                    return Json(res_ngm, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadNguoiGui()
        {
            var session_acc = SessionHelper.GetSession();
            if (session_acc == null)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(session_acc, JsonRequestBehavior.AllowGet);
            }

        }

        public async Task<ActionResult> CreateOrder_GHN(GHN_Ship ship)
        {
            var dao_ghn = new GHN_DAO();
            var res_ghn = dao_ghn.get_ghn_id(ship.id_ship);
            if (ship.note == null) ship.note = "Shippings";
            //object data = new
            //{
            //    payment_type_id = res_ghn.payment_type_id,
            //    note= ship.note,
            //    required_note= ship.required_note,
            //    return_phone= ship.return_phone,
            //    return_address= ship.return_address,
            //    return_district_id= 1450,
            //    return_ward_code= 20804,
            //    client_order_code= "",
            //    to_name= res_ghn.to_name,
            //    to_phone= res_ghn.to_phone,
            //    to_address= res_ghn.to_address,
            //    to_ward_code= res_ghn.to_ward_code,
            //    to_district_id= res_ghn.to_name_district,
            //    cod_amount= res_ghn.cod_amount,
            //    //content= res_ghn.,
            //    weight= res_ghn.weight*1000,
            //    length= res_ghn.length,
            //    width= res_ghn.width,
            //    height= res_ghn.height,
            //    pick_station_id= res_ghn.pick_station_id,
            //    insurance_value= res_ghn.insurance_value,
            //    service_id= res_ghn.service_id,
            //    //service_type_id= res_ghn.
            //};

            //var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            var content_ = "payment_type_id=" + res_ghn.payment_type_id + "&note=" + ship.note + "&required_note=" + ship.required_note +
                            "&return_phone=" + ship.return_phone + "&return_address=" + ship.return_address + "&return_district_id=" + 1450 + "&return_ward_code=" + 20804 + "&client_order_code=null" +
                            "&to_name=" + res_ghn.to_name + "&to_phone=" + res_ghn.to_phone + "&to_address=" + res_ghn.to_address + "&to_ward_code=" + res_ghn.to_ward_code + "&to_district_id=" + res_ghn.to_name_district + "&cod_amount=" + res_ghn.cod_amount +
                            "&content=" + "sanphamtengi" + "&weight=" + res_ghn.weight * 1000 + "&length=" + res_ghn.length + "&width=" + res_ghn.width + "&height=" + res_ghn.height + "&pick_station_id=" + res_ghn.pick_station_id + "&insurance_value=" + res_ghn.insurance_value + "&service_id=" + res_ghn.service_id;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("token", "ef26267b-c8c7-11ea-b16d-9289328232ea");
            httpClient.DefaultRequestHeaders.Add("ShopId", "1257313");

            HttpResponseMessage result = httpClient.PostAsync("https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create?" + content_, null).Result;
            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                var a = result.Content.ReadAsStringAsync().Result;
                //var deserialized = JsonConvert.DeserializeObject<object>(response);
                string[] arrListStr = response.Split(new char[] { '\"', '"' });

                var dao_hd = new HoaDon_DAO();
                var res_hd = dao_hd.update_hoadon_maghn(res_ghn.id_hoadon, arrListStr[11]);

                var dao_hdct = new HoaDonCT_DAO();
                var res_hdct = dao_hdct.get_hdct_hoadon_(res_ghn.id_hoadon); //trừ đi sl san pham

                if (arrListStr[11] == "" || arrListStr[11] == "0")
                {
                    return RedirectToAction("QuanLyHoaDon");
                }

                return RedirectToAction("QuanLyHoaDon");
            }

            return RedirectToAction("QuanLyHoaDon");
        }
        public void GiaoHang(string mgh)
        {
            Response.Redirect("https://khachhang.ghn.vn/order/edit?type=edit&index=0&id=GA7PAAX8" + mgh);
        }
        public void Urlpaypal(string magiaodich)
        {
            Response.Redirect("https://www.sandbox.paypal.com/activity/payment/" + magiaodich);
        }
        #endregion
    }
}