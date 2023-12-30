using CamTrai.Common;
using Model.DAO;
using Model.EF;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using CamTrai.Areas.Admin.Models;
using Nethereum.Util;
using System.Collections.Generic;
using PagedList;

namespace CamTrai.Areas.Admin.Controllers
{
   
    public class SinhVienController : AllController
    {
        private readonly SmartContractService _smartContractService;

        public SinhVienController()
        {
            _smartContractService = new SmartContractService();
        }

        // GET: Admin/SinhVien
        // GET: Admin/SinhVien
        public async Task<ActionResult> Index(string searchString, int page = 1, int pageSize = 10)
        {
            try
            {
                var dao = new SinhVienDao();
                var model = dao.ListAllPaging(searchString, page, pageSize);
                // Chuyển đổi danh sách thành đối tượng kiểu IPagedList
                var pagedModel = model.ToPagedList(page, pageSize);

                ViewBag.SearchString = searchString;

                //// Gọi smart contract để lấy hash code và gán cho từng sinh viên
                //foreach (var sv in model)
                //{
                //    try
                //    {
                //        // Gọi SmartContractService để lấy giá trị HashCode từ smart contract
                //        string hashedInfo = await _smartContractService.GetHashFromSmartContractAsync(sv.Mssv);
                //        sv.HashCode = hashedInfo;

                //        // Log thông tin của sinh viên
                //        Console.WriteLine($"Sinh Viên Mã số {sv.Mssv} - HashCode: {sv.HashCode}");
                //    }
                //    catch (Exception ex)
                //    {
                //        // Log lỗi hoặc hiển thị thông báo lỗi theo cách bạn muốn
                //        Console.WriteLine($"Error getting hash from smart contract for MSSV {sv.Mssv}: {ex.Message}");
                //        sv.HashCode = "Error";
                //    }
                //}

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Error in Index method: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tải danh sách Sinh Viên");
                return View(new List<tb_SinhVien>().ToPagedList(1, 10));
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Edit(int id)
        {
            var sv = new SinhVienDao().ViewDetail(id);
            return View(sv);
        }
        [HttpPost]

        public async Task<ActionResult> Create(tb_SinhVien SV)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dao = new SinhVienDao();

                    // Thiết lập giá trị EditCount mặc định nếu không được cung cấp
                    SV.EditCount = SV.EditCount ?? 0;

                    // Thiết lập giá trị LastEditTime là thời gian hiện tại
                    SV.LastEditTime = DateTime.Now;

                    long id = dao.Insert(SV);

                    if (id > 0)
                    {
                        // Thêm Sinh Viên thành công
                        string studentInfoToHash = $"{SV.Ho}{SV.Ten}{SV.GioiTinh}{SV.Mssv}{SV.DiaChi}{SV.NgaySinh}{SV.MaLop}{SV.DiemGPA}{SV.Image}";
                        string hashedInfo = CalculateHash(studentInfoToHash);

                        // Đặt giá trị HashCode, số lần sửa đổi và thời gian sửa đổi cho sinh viên mới thêm vào
                        SV.HashCode = hashedInfo;
                        SV.EditCount = 0;
                        SV.LastEditTime = DateTime.Now;

                        // Cập nhật giá trị HashCode vào CSDL
                        if (dao.Update(SV))
                        {
                            // Gọi Smart Contract để thêm thông tin sinh viên vào blockchain
                            await SendHashAndMSSVToSmartContractAsync(SV.Mssv, hashedInfo);
                            dao.UpdateEditInfoAsync(id, newEditCount: 0, lastEditTime: DateTime.Now);
                            // Cập nhật HashCode thành công, chuyển hướng đến trang Index
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            // Nếu có lỗi khi cập nhật CSDL, có thể xử lý hoặc thông báo lỗi tùy theo yêu cầu
                            ModelState.AddModelError("", "Cập nhật HashCode không thành công");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Thêm Sinh Viên không thành công");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Error in Create method: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm Sinh Viên");
            }

            return View(SV);
        }


        [HttpPost]
        public async Task<ActionResult> Edit(tb_SinhVien SV)
        {
            if (ModelState.IsValid)
            {
                var dao = new SinhVienDao();
                var currentSv = dao.ViewDetail(SV.ID);

                if (currentSv != null)
                {
                    // Tăng giá trị EditCount và cập nhật thời gian chỉnh sửa
                    int newEditCount = currentSv.EditCount.HasValue ? currentSv.EditCount.Value + 1 : 1;

                    if (await dao.UpdateEditInfoAsync(SV.ID, newEditCount, DateTime.Now))
                    {
                        // Cập nhật Sinh Viên thành công
                        string studentInfoToHash = $"{SV.Ho}{SV.Ten}{SV.GioiTinh}{SV.Mssv}{SV.DiaChi}{SV.NgaySinh}{SV.MaLop}{SV.DiemGPA}{SV.Image}";
                        string hashedInfo = CalculateHash(studentInfoToHash);

                        // Cập nhật HashCode vào CSDL
                        SV.HashCode = hashedInfo;
                        if (dao.Update(SV))
                        {
                            // Gọi Smart Contract để thêm thông tin sinh viên vào blockchain
                            await SendHashAndMSSVToSmartContractAsync(SV.Mssv, hashedInfo);

                            // Chuyển hướng đến trang Index
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Cập nhật Sinh Viên không thành công");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Cập nhật EditCount và LastEditTime không thành công");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy Sinh Viên để cập nhật");
                }
            }
            return View("Index");
        }

        public static string CalculateHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async Task SendHashAndMSSVToSmartContractAsync(string mssv, string hashedInfo)
        {
            try
            {
                var web3 = new Web3("http://localhost:8545");
                var contractAddress = "0xd9145CCE52D386f254917e481eB44e9943F39138";
                var senderAddress = "0x8AB812f8EE5bDF5BAe7d0741Df36298094911294";

                // Lấy thông tin tài khoản mặc định từ nút Ethereum
                var defaultAccount = await web3.Eth.CoinBase.SendRequestAsync();

                // Nếu tài khoản mặc định không được xác định, hãy sử dụng địa chỉ gửi thông qua tham số
                var fromAddress = defaultAccount ?? senderAddress;

                var gasPrice = Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei);
                var gas = await web3.Eth.Transactions.EstimateGas.SendRequestAsync(new Nethereum.RPC.Eth.DTOs.CallInput
                {
                    To = contractAddress,
                    Data = "0x", // Sử dụng data tương ứng với hàm 'addSinhVien'
                    From = fromAddress,
                    Gas = new HexBigInteger(21000)
                });

                var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput
                {
                    Gas = new HexBigInteger(gas),
                    GasPrice = new HexBigInteger(gasPrice),
                    To = contractAddress,
                    From = fromAddress,
                    Value = new HexBigInteger(0),
                    Data = "0x" + "addSinhVien" + mssv + hashedInfo // Cần sửa lại data tương ứng với hàm 'addSinhVien'
                };

                var transactionHash = await web3.Eth.Transactions.SendTransaction.SendRequestAsync(transactionInput);
                Console.WriteLine($"Transaction Hash: {transactionHash}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending transaction: {ex.Message}");
            }
        }

       

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            new SinhVienDao().Delete(id);
            return RedirectToAction("Index");
        }
    }
}
