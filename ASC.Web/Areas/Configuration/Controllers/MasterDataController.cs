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
            var currentUser = User.Identity?.Name ?? "admin";

            var name = model.MasterKeyInContext?.Name?.Trim();
            var isActive = ReadFormBool("MasterKeyInContext.IsActive");

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Name không được để trống.");

                var masterKeys = await _masterData.GetAllMasterKeysAsync();
                model.MasterKeys = _mapper.Map<List<MasterDataKeyViewModel>>(masterKeys);

                return View(model);
            }

            if (model.IsEdit)
            {
                var rowKey = model.MasterKeyInContext.RowKey?.Trim();
                var partitionKey = model.MasterKeyInContext.PartitionKey?.Trim();

                if (string.IsNullOrWhiteSpace(rowKey) || string.IsNullOrWhiteSpace(partitionKey))
                {
                    ModelState.AddModelError("", "Không tìm thấy RowKey hoặc PartitionKey để cập nhật.");

                    var masterKeys = await _masterData.GetAllMasterKeysAsync();
                    model.MasterKeys = _mapper.Map<List<MasterDataKeyViewModel>>(masterKeys);

                    return View(model);
                }

                var entity = new MasterDataKey
                {
                    RowKey = rowKey,
                    PartitionKey = partitionKey,
                    Name = name,
                    IsActive = isActive,
                    IsDeleted = false,
                    UpdatedDate = DateTime.Now,
                    UpdatedBy = currentUser
                };

                await _masterData.UpdateMasterKeyAsync(partitionKey, entity);
            }
            else
            {
                var entity = new MasterDataKey
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = name,
                    Name = name,
                    IsActive = isActive,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    CreatedBy = currentUser,
                    UpdatedBy = currentUser
                };

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
        public async Task<IActionResult> MasterValuesByKey(string? key)
        {
            List<MasterDataValue> values;

            if (string.IsNullOrWhiteSpace(key) || key == "--Select--" || key == "--All--")
            {
                values = await _masterData.GetAllMasterValuesAsync();
            }
            else
            {
                values = await _masterData.GetAllMasterValuesByKeyAsync(key.Trim());
            }

            values ??= new List<MasterDataValue>();

            var result = values.Select(x => new
            {
                rowKey = x.RowKey,
                partitionKey = x.PartitionKey,
                name = x.Name,
                isActive = x.IsActive
            });

            return Json(new { data = result });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveMasterValue()
        {
            try
            {
                var currentUser = User.Identity?.Name ?? "admin";

                var isEditText = Request.Form["isEdit"].ToString();
                var rowKey = Request.Form["rowKey"].ToString()?.Trim();
                var partitionKey = Request.Form["partitionKey"].ToString()?.Trim();
                var name = Request.Form["name"].ToString()?.Trim();
                var isActiveText = Request.Form["isActive"].ToString();

                var isEdit = isEditText.Equals("true", StringComparison.OrdinalIgnoreCase);
                var isActive = isActiveText.Equals("true", StringComparison.OrdinalIgnoreCase)
                               || isActiveText.Equals("on", StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrWhiteSpace(partitionKey))
                {
                    return Json(new { success = false, text = "Partition Key không được để trống." });
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new { success = false, text = "Name không được để trống." });
                }

                if (isEdit)
                {
                    if (string.IsNullOrWhiteSpace(rowKey))
                    {
                        return Json(new { success = false, text = "RowKey không hợp lệ khi cập nhật." });
                    }

                    var masterDataValue = new MasterDataValue
                    {
                        PartitionKey = partitionKey,
                        RowKey = rowKey,
                        Name = name,
                        IsActive = isActive,
                        IsDeleted = false,
                        UpdatedDate = DateTime.Now,
                        UpdatedBy = currentUser
                    };

                    var result = await _masterData.UpdateMasterValueAsync(
                        masterDataValue.PartitionKey,
                        masterDataValue.RowKey,
                        masterDataValue);

                    if (!result)
                    {
                        return Json(new { success = false, text = "Không tìm thấy Master Value để cập nhật." });
                    }
                }
                else
                {
                    var masterDataValue = new MasterDataValue
                    {
                        RowKey = Guid.NewGuid().ToString(),
                        PartitionKey = partitionKey,
                        Name = name,
                        IsActive = isActive,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CreatedBy = currentUser,
                        UpdatedBy = currentUser
                    };

                    var result = await _masterData.InsertMasterValueAsync(masterDataValue);

                    if (!result)
                    {
                        return Json(new { success = false, text = "Tạo Master Value thất bại." });
                    }
                }

                return Json(new
                {
                    success = true,
                    text = isEdit ? "Cập nhật Master Value thành công." : "Tạo Master Value thành công."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, text = ex.GetBaseException().Message });
            }
        }

        private async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        {
            var masterValueList = new List<MasterDataValue>();
            var currentUser = User.Identity?.Name ?? "admin";

            using (var memoryStream = new MemoryStream())
            {
                await excelFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                ExcelPackage.License.SetNonCommercialOrganization("Thu Dau Mot University");

                using (var package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    if (worksheet.Dimension == null)
                    {
                        return masterValueList;
                    }

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var partitionKey = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var name = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var isActiveText = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                        if (string.IsNullOrWhiteSpace(partitionKey) &&
                            string.IsNullOrWhiteSpace(name) &&
                            string.IsNullOrWhiteSpace(isActiveText))
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(partitionKey))
                        {
                            throw new Exception($"Dòng {row}: MasterKey/PartitionKey không được để trống.");
                        }

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            throw new Exception($"Dòng {row}: MasterValue/Name không được để trống.");
                        }

                        if (!bool.TryParse(isActiveText ?? "false", out bool isActive))
                        {
                            throw new Exception($"Dòng {row}: IsActive phải là TRUE hoặc FALSE.");
                        }

                        var masterDataValue = new MasterDataValue
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = partitionKey,
                            Name = name,
                            IsActive = isActive,
                            IsDeleted = false,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            CreatedBy = currentUser,
                            UpdatedBy = currentUser
                        };

                        masterValueList.Add(masterDataValue);
                    }
                }
            }

            return masterValueList;
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveMasterKey()
        {
            try
            {
                var currentUser = User.Identity?.Name ?? "admin";

                var isEditText = Request.Form["isEdit"].ToString();
                var rowKey = Request.Form["rowKey"].ToString()?.Trim();
                var partitionKey = Request.Form["partitionKey"].ToString()?.Trim();
                var name = Request.Form["name"].ToString()?.Trim();
                var isActiveText = Request.Form["isActive"].ToString();

                var isEdit = isEditText.Equals("true", StringComparison.OrdinalIgnoreCase);

                var isActive = isActiveText.Equals("true", StringComparison.OrdinalIgnoreCase)
                               || isActiveText.Equals("on", StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new { success = false, text = "Name không được để trống." });
                }

                if (isEdit)
                {
                    if (string.IsNullOrWhiteSpace(rowKey) || string.IsNullOrWhiteSpace(partitionKey))
                    {
                        return Json(new
                        {
                            success = false,
                            text = "RowKey hoặc PartitionKey không hợp lệ khi cập nhật."
                        });
                    }

                    var entity = new MasterDataKey
                    {
                        RowKey = rowKey,
                        PartitionKey = partitionKey,
                        Name = name,
                        IsActive = isActive,
                        IsDeleted = false,
                        UpdatedDate = DateTime.Now,
                        UpdatedBy = currentUser
                    };

                    var result = await _masterData.UpdateMasterKeyAsync(partitionKey, entity);

                    if (!result)
                    {
                        return Json(new
                        {
                            success = false,
                            text = "Không tìm thấy Master Key để cập nhật."
                        });
                    }
                }
                else
                {
                    var entity = new MasterDataKey
                    {
                        RowKey = Guid.NewGuid().ToString(),
                        PartitionKey = name,
                        Name = name,
                        IsActive = isActive,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CreatedBy = currentUser,
                        UpdatedBy = currentUser
                    };

                    var result = await _masterData.InsertMasterKeyAsync(entity);

                    if (!result)
                    {
                        return Json(new
                        {
                            success = false,
                            text = "Tạo Master Key thất bại."
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    text = isEdit ? "Cập nhật Master Key thành công." : "Tạo Master Key thành công."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    text = ex.GetBaseException().Message
                });
            }
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            try
            {
                var files = Request.Form.Files;

                if (files == null || !files.Any())
                {
                    return Json(new { success = false, text = "Chưa chọn file upload." });
                }

                var excelFile = files.First();

                if (excelFile.Length <= 0)
                {
                    return Json(new { success = false, text = "File rỗng." });
                }

                var extension = Path.GetExtension(excelFile.FileName)?.ToLower();

                if (extension != ".xlsx" && extension != ".xls")
                {
                    return Json(new { success = false, text = "Chỉ hỗ trợ file Excel .xlsx hoặc .xls." });
                }

                var masterData = await ParseMasterDataExcel(excelFile);

                if (masterData == null || !masterData.Any())
                {
                    return Json(new { success = false, text = "File không có dữ liệu hợp lệ." });
                }

                var result = await _masterData.UploadBulkMasterData(masterData);

                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        text = "Upload thất bại. Kiểm tra lại dữ liệu trong file Excel."
                    });
                }

                return Json(new { success = true, text = "Upload thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, text = ex.GetBaseException().Message });
            }
        }

        private bool ReadFormBool(string key)
        {
            var values = Request.Form[key];

            foreach (var value in values)
            {
                if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}