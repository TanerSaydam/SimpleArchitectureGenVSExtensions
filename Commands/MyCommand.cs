using System.IO;

namespace SimpleArchitectureGen
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();

            string message = "";
            string results = "";
            string path = "";
            string selectedFileName = "";

            string[] strings = FindPathAndName(docView);
            if (strings[0] == "Error")
            {
                _ = VS.MessageBox.ShowWarningAsync("Seçtiğiniz dosya Entities katmanında Concrete klasöründe bulunan bir entity olmalıdır!");
                return;
            }

            path = strings[0];
            selectedFileName = strings[1];



            string question =
                "I" + selectedFileName + "Dal " + "\n" +
                "Ef" + selectedFileName + "Dal " + "\n" +
                "DbSet<" + selectedFileName + "> " + "\n" +
                "I" + selectedFileName + "Service " + "\n" +
                selectedFileName + "Manager " + "\n" +
                selectedFileName + "Validator " + "\n" +
                selectedFileName + "Messages ";

            if (!await VS.MessageBox.ShowConfirmAsync(selectedFileName + " entity için aşağıdaki dosyalar otomatik olarak oluşturulacaktır. Onaylıyor musunuz?", question))
            {
                return;
            }

            message = CreateDataAccessInterfaceDal(path, selectedFileName);

            results = "\n" + "1) " + message;

            message = CreateDataAccessClassDal(path, selectedFileName);

            results = results + "\n" + "2) " + message;

            message = AddDbSetForClass(path, selectedFileName);

            results = results + "\n" + "3) " + message;

            message = CreateBusinessInterfaceService(path, selectedFileName);

            results = results + "\n" + "4) " + message;

            message = CreateBusinessClassManager(path, selectedFileName);

            results = results + "\n" + "5) " + message;

            message = CreateBusinessClassValidator(path, selectedFileName);

            results = results + "\n" + "6) " + message;

            message = CreateBusinessClassMessages(path, selectedFileName);

            results = results + "\n" + "7) " + message;

            message = ChangeBusinessDependencyResolversModule(path, selectedFileName);

            results = results + "\n" + "8) " + message;

            message = CreateWebApiController(path, selectedFileName);

            results = results + "\n" + "9) " + message;

            await VS.MessageBox.ShowAsync(results);
        }

        private static string[] FindPathAndName(DocumentView docView)
        {
            string path = docView.FilePath;
            path = path.Replace(@"\", "/");
            string[] paths = path.Split('/');
            int count = paths.Length;

            if (paths[count - 3] != "Entities" && paths[count - 2] != "Concrete")
            {
                string[] errors = { "Error" };
                return errors;
            }

            if (count < 2)
            {
                _ = VS.MessageBox.ShowWarningAsync("Seçtiğiniz dosya dönüştülrebilir değil! Seçilen dosya: " + paths[0], "Hata!");
            }

            for (int i = 0; i < (count - 3); i++)
            {
                if (i > 0)
                {
                    path = path + "/" + paths[i];
                }
                else
                {
                    path = paths[i];
                }
            }

            //string projectName = paths[count - 4];
            string projePath = path + "/";
            string selectedFileName = paths[count - 1];

            string[] strings = { projePath, selectedFileName };

            return strings;
        }

        private static string CreateDataAccessInterfaceDal(string path, string selectedFileName)
        {
            try
            {
                string directoryPath = path;
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = "I" + className + "Dal";
                string interfacePath = "DataAccess/Repositories/" + className + "Repository";
                path = path + interfacePath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
                }

                directoryPath = directoryPath + interfacePath;
                bool isDirectoryExtists = Directory.Exists(directoryPath);
                if (!isDirectoryExtists)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                interfacePath = interfacePath.Replace("/", ".");

                string[] contents =
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using System.Text;",
                    "using System.Threading.Tasks;",
                    "using Core.DataAccess;",
                    "using Entities.Concrete;",
                    "",
                    "namespace " + interfacePath,
                    "{",
                    "    public interface " + fileName + " : IEntityRepository<" + className + ">",
                    "    {",
                    "    }",
                    "}"
                };

                System.IO.File.AppendAllLines(path, contents);
                return fileName + " oluşturuldu";
            }
            catch (Exception)
            {
                return "DataAccess için Interface Dal oluştururken bir hatayla karşılaştık!";
                throw;
            }
        }

        private static string CreateDataAccessClassDal(string path, string selectedFileName)
        {
            try
            {
                string directoryPath = path;
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = "Ef" + className + "Dal";
                string classPath = "DataAccess/Repositories/" + className + "Repository";
                path = path + classPath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
                }

                directoryPath = directoryPath + classPath;
                bool isDirectoryExtists = Directory.Exists(directoryPath);
                if (!isDirectoryExtists)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                classPath = classPath.Replace("/", ".");

                string[] contents =
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using System.Text;",
                    "using System.Threading.Tasks;",
                    "using Core.DataAccess.EntityFramework;",
                    "using Entities.Concrete;",
                    "using DataAccess.Repositories." + className + "Repository;",
                    "using DataAccess.Context.EntityFramework;",
                    "",
                    "namespace " + classPath,
                    "{",
                    "    public class " + fileName + " : EfEntityRepositoryBase<" + className + ", SimpleContextDb>, I" + className + "Dal",
                    "    {",
                    "    }",
                    "}"
                };

                System.IO.File.AppendAllLines(path, contents);
                return fileName + " başarıyla oluşturuldu";
            }
            catch (Exception)
            {
                return "DataAccess için Class Dal oluştururken bir hatayla karşılaştık!";
                throw;
            }
        }

        private static string AddDbSetForClass(string path, string selectedFileName)
        {
            try
            {
                path = path + "DataAccess/Context/EntityFramework/SimpleContextDb.cs";

                if (!System.IO.File.Exists(path))
                {
                    return "Context dosyası bulunamadı! DbSet işlemi iptal edildi";
                }

                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string classDatabaseName = className + "s";
                if (className.Substring((className.Length - 1), 1) == "s")
                {
                    classDatabaseName = className + "es";
                }
                if (className.Substring((className.Length - 1), 1) == "y")
                {
                    classDatabaseName = className.Substring(0, (className.Length - 1)) + "ies";
                }

                string dbSet = "        public DbSet<" + className + ">? " + classDatabaseName + " { get; set; }";

                var result = System.IO.File.ReadAllLines(path);
                int resultLength = result.Length;
                bool isDbSetAdded = false;
                int replaceIndex = 0;
                int newResultLength = resultLength + 1;
                for (int i = 0; i < resultLength; i++)
                {
                    var isDbSetExsist = result[i].Contains(className);
                    if (isDbSetExsist)
                    {
                        isDbSetAdded = true;
                    }

                    if (result[i].Contains("{ get; set; }"))
                    {
                        replaceIndex = i;
                    }
                }

                replaceIndex = replaceIndex + 1;

                if (isDbSetAdded)
                    return "Daha önce DbSet yapıldığı için tekrar yapılmadı!";

                string[] newResult = new string[newResultLength];
                int count = 0;
                for (int i = 0; i < (newResultLength); i++)
                {

                    if (i == replaceIndex && !isDbSetAdded)
                    {
                        newResult[i] = dbSet;
                    }
                    else
                    {
                        newResult[i] = result[count];
                        count++;
                    }
                }

                System.IO.File.Delete(path);
                System.IO.File.AppendAllLines(path, newResult);

                return "DbSet başarıyla oluşturuldu";
            }
            catch (Exception)
            {
                return "DataAccess katmanında DbSet oluştururken bir hatayla karşılaştık!";
                throw;
            }

        }

        private static string CreateBusinessInterfaceService(string path, string selectedFileName)
        {
            try
            {
                string directoryPath = path;
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string lowerClassName = className.Substring(0, 1).ToLower() + className.Substring(1);
                string fileName = "I" + className + "Service";
                string interfacePath = "Business/Repositories/" + className + "Repository";
                path = path + interfacePath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
                }

                directoryPath = directoryPath + interfacePath;
                bool isDirectoryExtists = Directory.Exists(directoryPath);
                if (!isDirectoryExtists)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                interfacePath = interfacePath.Replace("/", ".");

                string[] contents =
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using System.Text;",
                    "using System.Threading.Tasks;",
                    "using Entities.Concrete;",
                    "using Core.Utilities.Result.Abstract;",
                    "",
                    "namespace " + interfacePath,
                    "{",
                    "    public interface " + fileName,
                    "    {",
                    "        Task<IResult> Add(" + className + " " + lowerClassName + ");",
                    "        Task<IResult> Update(" + className + " " + lowerClassName + ");",
                    "        Task<IResult> Delete(" + className + " " + lowerClassName + ");",
                    "        Task<IDataResult<List<" + className + ">>> GetList();",
                    "        Task<IDataResult<" + className + ">> GetById(int id);",
                    "    }",
                    "}"
                };

                System.IO.File.AppendAllLines(path, contents);
                return fileName + " dosya başarıyla oluşturuldu";
            }
            catch (Exception)
            {
                return "Business için Interface Service oluştururken bir hatayla karşılaştık!";
                throw;
            }
        }

        private static string CreateBusinessClassManager(string path, string selectedFileName)
        {
            try
            {
                string directoryPath = path;
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string lowerClassName = className.Substring(0, 1).ToLower() + className.Substring(1);
                string fileName = className + "Manager";
                string classPath = "Business/Repositories/" + className + "Repository";
                path = path + classPath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
                }

                directoryPath = directoryPath + classPath;
                bool isDirectoryExtists = Directory.Exists(directoryPath);
                if (!isDirectoryExtists)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                classPath = classPath.Replace("/", ".");

                string _className = "_" + className.Substring(0, 1).ToLower() + className.Substring(1, className.Length - 1) + "Dal";

                string classNameForConstructor = className.Substring(0, 1).ToLower() + className.Substring(1, className.Length - 1) + "Dal";

                string[] contents =
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using System.Text;",
                    "using System.Threading.Tasks;",
                    "using Business.Repositories." + className + "Repository;",
                    "using Entities.Concrete;",
                    "using Business.Aspects.Secured;",
                    "using Core.Aspects.Validation;",
                    "using Core.Aspects.Caching;",
                    "using Core.Aspects.Performance;",
                    "using Business.Repositories." + className + "Repository.Validation;",
                    "using Business.Repositories." + className + "Repository.Constants;",
                    "using Core.Utilities.Result.Abstract;",
                    "using Core.Utilities.Result.Concrete;",
                    "using DataAccess.Repositories." + className + "Repository;",
                    "",
                    "namespace " + classPath,
                    "{",
                    "    public class " + fileName + " : I" + className + "Service",
                    "    {",
                    "        private readonly I" + className + "Dal " + _className + ";",
                    "",
                    "        public " + fileName + "(I" + className + "Dal " + classNameForConstructor + ")",
                    "        {",
                    "            " + _className + " = " + classNameForConstructor + ";",
                    "        }",
                    "",
                    "        [SecuredAspect()]",
                    "        [ValidationAspect(typeof(" + className +"Validator))]",
                    @"        [RemoveCacheAspect(""I" + className + @"Service.Get"")]",
                    "",
                    "        public async Task<IResult> Add(" + className + " " + lowerClassName + ")",
                    "        {",
                    "            await _" + lowerClassName + "Dal.Add(" + lowerClassName + ");",
                    "            return new SuccessResult(" + className + "Messages.Added);",
                    "        }",
                    "",
                    "        [SecuredAspect()]",
                    "        [ValidationAspect(typeof(" + className +"Validator))]",
                    @"        [RemoveCacheAspect(""I" + className + @"Service.Get"")]",
                    "",
                    "        public async Task<IResult> Update(" + className + " " + lowerClassName + ")",
                    "        {",
                    "            await _" + lowerClassName + "Dal.Update(" + lowerClassName + ");",
                    "            return new SuccessResult(" + className + "Messages.Updated);",
                    "        }",
                    "",
                    "        [SecuredAspect()]",
                    @"        [RemoveCacheAspect(""I" + className + @"Service.Get"")]",
                    "",
                    "        public async Task<IResult> Delete(" + className + " " + lowerClassName + ")",
                    "        {",
                    "            await _" + lowerClassName + "Dal.Delete(" + lowerClassName + ");",
                    "            return new SuccessResult(" + className + "Messages.Deleted);",
                    "        }",
                    "",
                    "        [SecuredAspect()]",
                    "        [CacheAspect()]",
                    "        [PerformanceAspect()]",
                    "        public async Task<IDataResult<List<" + className + ">>> GetList()",
                    "        {",
                    "            return new SuccessDataResult<List<" + className +">>(await _" + lowerClassName + "Dal.GetAll());",
                    "        }",
                    "",
                    "        [SecuredAspect()]",
                    "        public async Task<IDataResult<" + className + ">> GetById(int id)",
                    "        {",
                    "            return new SuccessDataResult<" + className +">(await _" + lowerClassName + "Dal.Get(p => p.Id == id));",
                    "        }",
                    "",
                    "    }",
                    "}"
                };

                System.IO.File.AppendAllLines(path, contents);
                return fileName + " dosya başarıyla oluşturuldu";
            }
            catch (Exception)
            {
                return "Business için Class Manager oluştururken bir hatayla karşılaştık!";
                throw;
            }
        }

        private static string CreateBusinessClassValidator(string path, string selectedFileName)
        {
            try
            {
                string directoryPath = path;
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = className + "Validator";
                string classPath = "Business/Repositories/" + className + "Repository/Validation";
                path = path + classPath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
                }

                directoryPath = directoryPath + classPath;
                bool isDirectoryExtists = Directory.Exists(directoryPath);
                if (!isDirectoryExtists)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                classPath = classPath.Replace("/", ".");

                string[] contents =
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using FluentValidation;",
                    "using System.Text;",
                    "using System.Threading.Tasks;",
                    "using Entities.Concrete;",
                    "",
                    "namespace " + classPath,
                    "{",
                    "    public class " + fileName + " : AbstractValidator<" + className + ">",
                    "    {",
                    "        public " + fileName + "()",
                    "        {",
                    "        }",
                    "    }",
                    "}"
                };

                System.IO.File.AppendAllLines(path, contents);
                return fileName + " dosya başarıyla oluşturuldu"; ;
            }
            catch (Exception)
            {
                return "Business için Class Validator oluştururken bir hatayla karşılaştık!";
                throw;
            }
        }

        private static string CreateBusinessClassMessages(string path, string selectedFileName)
        {
            try
            {
                string directoryPath = path;
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = className + "Messages";
                string classPath = "Business/Repositories/" + className + "Repository/Constants";
                path = path + classPath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
                }

                directoryPath = directoryPath + classPath;
                bool isDirectoryExtists = Directory.Exists(directoryPath);
                if (!isDirectoryExtists)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                classPath = classPath.Replace("/", ".");

                string[] contents =
                {
            "using System;",
            "using System.Collections.Generic;",
            "using FluentValidation;",
            "using System.Text;",
            "using System.Threading.Tasks;",
            "",
            "namespace " + classPath,
            "{",
            "    public class " + fileName,
            "    {",
            @"        public static string Added = ""Kayıt işlemi başarılı"";",
            @"        public static string Updated = ""Güncelleme işlemi başarılı"";",
            @"        public static string Deleted = ""Silme işlemi başarılı"";",
            "    }",
            "}"
        };

                System.IO.File.AppendAllLines(path, contents);
                return fileName + " dosya başarıyla oluşturuldu"; ;
            }
            catch (Exception)
            {
                return "Business için Class Messages oluştururken bir hatayla karşılaştık!";
                throw;
            }
        }

        private static string ChangeBusinessDependencyResolversModule(string path, string selectedFileName)
        {
            try
            {
                path = path + "Business/DependencyResolvers/Autofac/AutofacBusinessModule.cs";

                if (!System.IO.File.Exists(path))
                {
                    return "AutofacBusinessModule bulunamadı! İşleme devam edilemiyor";
                }

                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string managerName = className + "Manager";
                string serviceName = "I" + className + "Service";
                string classDal = "Ef" + className + "Dal";
                string interfaceDal = "I" + className + "Dal";

                string registerType1 = "            builder.RegisterType<" + managerName + ">().As<" + serviceName + ">().SingleInstance();";
                string registerType2 = "            builder.RegisterType<" + classDal + ">().As<" + interfaceDal + ">().SingleInstance();";

                var result = System.IO.File.ReadAllLines(path);
                int resultLength = result.Length;
                bool addManager = true;
                bool addDal = true;
                int replaceIndex = 0;
                int lastUsingIndex1 = 0;
                int lastUsingIndex2 = 0;
                int newResultLength = resultLength + 5;
                for (int i = 0; i < resultLength; i++)
                {
                    var isManagerExsist = result[i].Contains(managerName);
                    if (isManagerExsist)
                    {
                        addManager = false;
                        newResultLength--;
                    }

                    var isDalExsist = result[i].Contains(classDal);
                    if (isDalExsist)
                    {
                        addDal = false;
                        newResultLength--;
                    }

                    if (result[i].Contains("var assembly ="))
                    {
                        replaceIndex = i;
                    }

                    if (result[i].Contains("namespace Business"))
                    {
                        lastUsingIndex1 = i;
                        lastUsingIndex2 = i;
                    }
                }

                lastUsingIndex1 = lastUsingIndex1 - 1;
                lastUsingIndex2 = lastUsingIndex2 - 2;
                replaceIndex = replaceIndex + 2;

                if (!addManager && !addDal)
                    return "Daha önce dependency injection yapıldığı için tekrar yapılmadı!";

                string[] newResult = new string[newResultLength];
                int count = 0;
                for (int i = 0; i < (newResultLength); i++)
                {
                    if (i == lastUsingIndex1 && addManager)
                    {
                        newResult[i] = "using DataAccess.Repositories." + className + "Repository;";
                    }
                    else if (i == lastUsingIndex2 && addManager)
                    {
                        newResult[i] = "using Business.Repositories." + className + "Repository;";
                    }
                    else if (i == replaceIndex && addManager)
                    {
                        newResult[i] = registerType1;
                    }
                    else if (i == replaceIndex + 1 && addDal)
                    {
                        newResult[i] = registerType2;
                    }
                    else if (i == replaceIndex + 2)
                    {
                        newResult[i] = "";
                    }
                    else
                    {
                        newResult[i] = result[count];
                        count++;
                    }
                }

                System.IO.File.Delete(path);
                System.IO.File.AppendAllLines(path, newResult);

                return "Dependency başarıyla oluşturuldu";
            }
            catch (Exception)
            {
                return "Business katmanında dependency oluştururken bir hatayla karşılaştık!";
                throw;
            }

        }

        private static string CreateWebApiController(string path, string selectedFileName)
        {
            try
            {
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string lowerClassName = className.Substring(0, 1).ToLower() + className.Substring(1);
                string fileName = className + "s" + "Controller";
                if (className.Substring((className.Length - 1), 1) == "s")
                {
                    fileName = className + "esController";
                }
                if (className.Substring((className.Length - 1), 1) == "y")
                {
                    fileName = className.Substring(0, (className.Length - 1)) + "iesController";
                }

                string classPath = "WebApi/Controllers";
                path = path + classPath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
                }

                classPath = classPath.Replace("/", ".");

                string _className = "_" + className.Substring(0, 1).ToLower() + className.Substring(1, className.Length - 1) + "Service";

                string classNameForConstructor = className.Substring(0, 1).ToLower() + className.Substring(1, className.Length - 1) + "Service";

                string[] contents =
                {
                "using Business.Repositories." + className + "Repository;",
                "using Entities.Concrete;",
                "using Microsoft.AspNetCore.Mvc;",
                "",
                "namespace " + classPath,
                "{",
                @"    [Route(""api/[controller]"")]",
                "    [ApiController]",
                "    public class " + fileName + " : ControllerBase",
                "    {",
                "        private readonly I" + className + "Service " + _className + ";",
                "",
                "        public " + fileName + "(I" + className + "Service " + classNameForConstructor + ")",
                "        {",
                "            " + _className + " = " + classNameForConstructor + ";",
                "        }",
                "",
                @"        [HttpPost(""[action]"")]",
                "        public async Task<IActionResult> Add(" + className + " " + lowerClassName + ")",
                "        {",
                "            var result = await _" + lowerClassName + "Service.Add(" + lowerClassName + ");",
                "            if (result.Success)",
                "            {",
                "                return Ok(result);",
                "            }",
                "            return BadRequest(result.Message);",
                "        }",
                "",
                @"        [HttpPost(""[action]"")]",
                "        public async Task<IActionResult> Update(" + className + " " + lowerClassName + ")",
                "        {",
                "            var result = await _" + lowerClassName + "Service.Update(" + lowerClassName + ");",
                "            if (result.Success)",
                "            {",
                "                return Ok(result);",
                "            }",
                "            return BadRequest(result.Message);",
                "        }",
                "",
                @"        [HttpPost(""[action]"")]",
                "        public async Task<IActionResult> Delete(" + className + " " + lowerClassName + ")",
                "        {",
                "            var result = await _" + lowerClassName + "Service.Delete(" + lowerClassName + ");",
                "            if (result.Success)",
                "            {",
                "                return Ok(result);",
                "            }",
                "            return BadRequest(result.Message);",
                "        }",
                "",
                @"        [HttpGet(""[action]"")]",
                "        public async Task<IActionResult> GetList()",
                "        {",
                "            var result = await _" + lowerClassName + "Service.GetList();",
                "            if (result.Success)",
                "            {",
                "                return Ok(result);",
                "            }",
                "            return BadRequest(result.Message);",
                "        }",
                "",
                @"        [HttpGet(""[action]/{id}"")]",
                "        public async Task<IActionResult> GetById(int id)",
                "        {",
                "            var result = await _" + lowerClassName + "Service.GetById(id);",
                "            if (result.Success)",
                "            {",
                "                return Ok(result);",
                "            }",
                "            return BadRequest(result.Message);",
                "        }",
                "",
                "    }",
                "}"
            };

                System.IO.File.AppendAllLines(path, contents);
                return fileName + " dosya başarıyla oluşturuldu";
            }
            catch (Exception)
            {
                return "WebApi için Controller oluştururken bir hatayla karşılaştık!";
                throw;
            }
        }
    }
}
