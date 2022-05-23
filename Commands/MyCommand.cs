using System.Text;

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
                selectedFileName + "Validator ";

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

            message = ChangeBusinessDependencyResolversModule(path, selectedFileName);

            results = results + "\n" + "7) " + message;

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
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = "I" + className + "Dal";
                string interfacePath = "DataAccess/Abstract";
                path = path + interfacePath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
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
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = "Ef" + className + "Dal";
                string classPath = "DataAccess/Concrete/EntityFramework";
                path = path + classPath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
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
                    "using DataAccess.Abstract;",
                    "using DataAccess.Concrete.EntityFramework.Context;",
                    "",
                    "namespace " + classPath,
                    "{",
                    "    public class " + fileName + " : EfEntityRepositoryBase<" + className + ", SimpleContextDb> , I" + className + "Dal",
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
                path = path + "DataAccess/Concrete/EntityFramework/Context/SimpleContextDb.cs";

                if (!System.IO.File.Exists(path))
                {
                    return "Context dosyası bulunamadı! DbSet işlemi iptal edildi";
                }

                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));

                string dbSet = "        public DbSet<" + className + "> " + className + "s { get; set; }";

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

                    if (result[i].Contains("//Buraya dokunma"))
                    {
                        replaceIndex = i;
                    }
                }

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
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = "I" + className + "Service";
                string interfacePath = "Business/Abstract";
                path = path + interfacePath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
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
                    "",
                    "namespace " + interfacePath,
                    "{",
                    "    public interface " + fileName,
                    "    {",
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
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = className + "Manager";
                string classPath = "Business/Concrete";
                path = path + classPath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
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
                    "using Business.Abstract;",
                    "using Entities.Concrete;",
                    "using DataAccess.Abstract;",
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
                int fileNameLength = selectedFileName.Length;
                string className = selectedFileName.Substring(0, (fileNameLength - 3));
                string fileName = className + "Validator";
                string classPath = "Business/ValidationRules/FluentValidation";
                path = path + classPath + "/" + fileName + ".cs";

                if (System.IO.File.Exists(path))
                {
                    return fileName + " daha önceden oluşturulduğundan tekrar oluşturulmadı!";
                }

                classPath = classPath.Replace("/", ".");

                string[] contents =
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using FluentValidation;;",
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

        private static string ChangeBusinessDependencyResolversModule(string path, string selectedFileName)
        {
            try
            {
                path = path + "Business/DependencyResolvers/Autofac/AutofacBusinessModule.cs";

                if (!System.IO.File.Exists(path))
                {
                    return "AutofacBusinessModule bulunamadı! İşleme devam edilemiyor";
                }

                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);

                //while (System.IO.File.ReadAllText(b, 0, b.Length) > 0)
                //{
                //    Console.WriteLine(temp.GetString(b));
                //}

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
                int newResultLength = resultLength + 3;
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

                    if (result[i].Contains("//Buraya dokunma"))
                    {
                        replaceIndex = i;
                    }
                }

                if (!addManager && !addDal)
                    return "Daha önce dependency injection yapıldığı için tekrar yapılmadı!";

                string[] newResult = new string[newResultLength];
                int count = 0;
                for (int i = 0; i < (newResultLength); i++)
                {

                    if (i == replaceIndex && addManager)
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
    }
}
