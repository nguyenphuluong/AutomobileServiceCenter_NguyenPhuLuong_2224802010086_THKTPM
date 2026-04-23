using System.ComponentModel;
using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Web.Areas.Configuration.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : Controller
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var vm = _mapper.Map<List<MasterDataKeyViewModel>>(masterKeys);

            return View(new MasterKeysViewModel
            {
                MasterKeys = vm,
                MasterKeyInContext = new MasterDataKeyViewModel(),
                IsEdit = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.MasterKeys ??= new List<MasterDataKeyViewModel>();
                return View(model);
            }

            var entity = _mapper.Map<MasterDataKey>(model.MasterKeyInContext);

            if (model.IsEdit)
            {
                await _masterData.UpdateMasterKeyAsync(entity.PartitionKey, entity);
            }
            else
            {
                entity.RowKey = Guid.NewGuid().ToString();
                entity.PartitionKey = entity.Name;
                await _masterData.InsertMasterKeyAsync(entity);
            }

            return RedirectToAction(nameof(MasterKeys));
        }

        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();

            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                MasterValueInContext = new MasterDataValueViewModel(),
                IsEdit = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            var values = await _masterData.GetAllMasterValuesByKeyAsync(key);
            return Json(new { data = values });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        {
            if (!ModelState.IsValid)
            {
                return Json("Error");
            }

            var masterDataValue = _mapper.Map<MasterDataValue>(masterValue);

            if (isEdit)
            {
                await _masterData.UpdateMasterValueAsync(
                    masterDataValue.PartitionKey,
                    masterDataValue.RowKey,
                    masterDataValue);
            }
            else
            {
                masterDataValue.RowKey = Guid.NewGuid().ToString();

                // Nếu project mày có extension này thì dùng
                // masterDataValue.CreatedBy = HttpContext.User.GetCurrentUserDetails().Name;

                await _masterData.InsertMasterValueAsync(masterDataValue);
            }

            return Json(true);
        }

        private async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        {
            var masterValueList = new List<MasterDataValue>();

            using (var memoryStream = new MemoryStream())
            {
                await excelFile.CopyToAsync(memoryStream);

                using (var package = new ExcelPackage(memoryStream))
                {
                    
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var masterDataValue = new MasterDataValue();
                        masterDataValue.RowKey = Guid.NewGuid().ToString();
                        masterDataValue.PartitionKey = worksheet.Cells[row, 1].Value?.ToString();
                        masterDataValue.Name = worksheet.Cells[row, 2].Value?.ToString();
                        masterDataValue.IsActive = bool.Parse(worksheet.Cells[row, 3].Value?.ToString() ?? "false");

                        masterValueList.Add(masterDataValue);
                    }
                }
            }

            return masterValueList;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            var files = Request.Form.Files;

            if (!files.Any())
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            var excelFile = files.First();

            if (excelFile.Length <= 0)
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            var masterData = await ParseMasterDataExcel(excelFile);
            var result = await _masterData.UploadBulkMasterData(masterData);

            return Json(new { Success = result });
        }
    }
}