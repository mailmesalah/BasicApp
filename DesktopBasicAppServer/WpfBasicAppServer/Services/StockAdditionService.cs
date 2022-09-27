using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using WpfAccountServerApp.General;

namespace WpfAccountServerApp.Services
{   
    public class StockAdditionService : IStockAddition
    {
        private string mBillType = "SA";

        public bool CreateBill(CStockAddition oStockAddition)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        ProductService ls = new ProductService();
                        BillNoService bs = new BillNoService();


                        int cbillNo = bs.ReadNextStockAdditionBillNo(oStockAddition.FinancialCode);
                        bs.UpdateStockAdditionBillNo(oStockAddition.FinancialCode,cbillNo+1);

                        List<string> barcodes = new BarcodeService().ReadBarcodes(oStockAddition.Details.Count);
                        
                        for (int i = 0; i < oStockAddition.Details.Count; i++)
                        {
                            product_transactions pt = new product_transactions();

                            pt.bill_no= cbillNo.ToString();
                            pt.bill_type = mBillType;
                            pt.bill_date_time = oStockAddition.BillDateTime;
                            pt.narration = oStockAddition.Narration;
                            pt.financial_code = oStockAddition.FinancialCode;

                            pt.serial_no = oStockAddition.Details.ElementAt(i).SerialNo;
                            pt.product_code = oStockAddition.Details.ElementAt(i).ProductCode;
                            pt.product = oStockAddition.Details.ElementAt(i).Product;
                            pt.purchase_unit = oStockAddition.Details.ElementAt(i).StockAdditionUnit;
                            pt.purchase_unit_code = oStockAddition.Details.ElementAt(i).StockAdditionUnitCode;
                            pt.purchase_unit_value = oStockAddition.Details.ElementAt(i).StockAdditionUnitValue;
                            pt.quantity = oStockAddition.Details.ElementAt(i).Quantity;
                            pt.purchase_rate = oStockAddition.Details.ElementAt(i).StockAdditionRate;
                            pt.mrp = oStockAddition.Details.ElementAt(i).MRP;
                            //get a barcode here
                            pt.barcode = barcodes.ElementAt(i);                            
                            pt.unit_code= oStockAddition.Details.ElementAt(i).StockAdditionUnitCode;
                            pt.unit_value= oStockAddition.Details.ElementAt(i).StockAdditionUnitValue;

                            dataB.product_transactions.Add(pt);                            
                        }

                        dataB.SaveChanges();
                        //Success
                        returnValue = true;

                        dataBTransaction.Commit();
                    }
                    catch(Exception e)
                    {                        
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }                
            }

            return returnValue;
        }

        public bool DeleteBill(string billNo,string financialCode)
        {
            bool returnValue = true;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        //Delete the transaction
                        //get barcodes already used in other transactions
                        BarcodeService bs = new BarcodeService();
                        List<string> usedBarcodes = ReadAlreadyUsedBarcodes(billNo,financialCode);
                        //Remove editable entries of transaction
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == mBillType && !usedBarcodes.Contains<string>(x.barcode));
                        dataB.product_transactions.RemoveRange(cpp);

                        //Updating serial numbers of entires that are already used
                        int serialNo = 1;
                        var tr = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == mBillType && usedBarcodes.Contains<string>(x.barcode));
                        foreach (var item in tr)
                        {
                            item.serial_no = serialNo++;
                        }

                        dataB.SaveChanges();                        
                        dataBTransaction.Commit();
                    }
                    catch
                    {
                        returnValue = false;
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }
            }
            return returnValue;
        }

        public CStockAddition ReadBill(string billNo,string financialCode)
        {
            CStockAddition ccp = null;

            using (var dataB = new Database9001Entities())
            {
                var cps = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode&&x.bill_type==mBillType).OrderBy(y=>y.serial_no);
                
                if (cps.Count() > 0)
                {
                    ccp = new CStockAddition();

                    var cp = cps.FirstOrDefault();
                    ccp.Id = cp.id;
                    ccp.BillNo = cp.bill_no;
                    ccp.BillDateTime = cp.bill_date_time;
                    ccp.Narration = cp.narration;
                    ccp.FinancialCode = cp.financial_code;

                    foreach (var item in cps)
                    {
                        ccp.Details.Add(new CStockAdditionDetails() { SerialNo=(int)item.serial_no,ProductCode=item.product_code,Product=item.product, StockAdditionUnit=item.purchase_unit, StockAdditionUnitCode=item.purchase_unit_code, StockAdditionUnitValue = (decimal)item.purchase_unit_value, Quantity=(decimal)item.quantity, StockAdditionRate = (decimal)item.purchase_rate, MRP = (decimal)item.mrp, Total=(decimal)(item.quantity*item.purchase_rate), Barcode = item.barcode});
                    }
                }
                
            }

            return ccp;
        }

        public int ReadNextBillNo(string financialCode)
        {
            
            BillNoService bns = new BillNoService();
            return bns.ReadNextStockAdditionBillNo(financialCode);
            
        }

        public bool UpdateBill(CStockAddition oStockAddition)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        //get barcodes already used in other transactions
                        BarcodeService bs = new BarcodeService();                        
                        List<string> usedBarcodes =ReadAlreadyUsedBarcodes(oStockAddition.BillNo,oStockAddition.FinancialCode);
                        //Remove editable entries of transaction
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == oStockAddition.BillNo&& x.financial_code==oStockAddition.FinancialCode&&x.bill_type==mBillType && !usedBarcodes.Contains<string>(x.barcode));
                        dataB.product_transactions.RemoveRange(cpp);
                        
                        int serialNo = 1;
                        for (int i = 0; i < oStockAddition.Details.Count; i++)
                        {
                            if (!usedBarcodes.Contains<string>(oStockAddition.Details.ElementAt(i).Barcode))
                            {
                                product_transactions pt = new product_transactions();

                                pt.bill_no = oStockAddition.BillNo;
                                pt.bill_type = mBillType;
                                pt.bill_date_time = oStockAddition.BillDateTime;
                                pt.narration = oStockAddition.Narration;
                                pt.financial_code = oStockAddition.FinancialCode;

                                pt.serial_no = serialNo++;
                                pt.product_code = oStockAddition.Details.ElementAt(i).ProductCode;
                                pt.product = oStockAddition.Details.ElementAt(i).Product;
                                pt.purchase_unit = oStockAddition.Details.ElementAt(i).StockAdditionUnit;
                                pt.purchase_unit_code = oStockAddition.Details.ElementAt(i).StockAdditionUnitCode;
                                pt.purchase_unit_value = oStockAddition.Details.ElementAt(i).StockAdditionUnitValue;
                                pt.quantity = oStockAddition.Details.ElementAt(i).Quantity;
                                pt.purchase_rate = oStockAddition.Details.ElementAt(i).StockAdditionRate;
                                pt.mrp = oStockAddition.Details.ElementAt(i).MRP;
                                //get a barcode here
                                //MessageBox.Show(oStockAddition.Details.ElementAt(i).Barcode);
                                pt.barcode = oStockAddition.Details.ElementAt(i).Barcode != "" ?  oStockAddition.Details.ElementAt(i).Barcode: bs.ReadNextBarcode();
                                pt.unit_code = oStockAddition.Details.ElementAt(i).StockAdditionUnitCode;
                                pt.unit_value = oStockAddition.Details.ElementAt(i).StockAdditionUnitValue;

                                dataB.product_transactions.Add(pt);
                            }
                        }

                        //Updating serial numbers of entires that are already used
                        var tr = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == oStockAddition.BillNo && x.financial_code == oStockAddition.FinancialCode && x.bill_type == mBillType && usedBarcodes.Contains<string>(x.barcode));
                        foreach (var item in tr)
                        {
                            item.serial_no = serialNo++;
                        }

                        dataB.SaveChanges();

                        //Success
                        returnValue = true;

                        dataBTransaction.Commit();
                    }
                    catch(Exception e)
                    {                        
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }
            }
            return returnValue;
        }

        private List<string> ReadAlreadyUsedBarcodes(string billNo,string fCode)
        {
            List<string> barcodes = new List<string>();
            List<string> usedBarcodes = new List<string>();
            using (var dataB = new Database9001Entities())
            {
                try
                {
                    var bar = dataB.product_transactions.Select(c => new { c.barcode, c.bill_type, c.bill_no, c.financial_code }).Where(x => x.bill_type == mBillType && x.bill_no == billNo && x.financial_code == fCode);
                    barcodes = bar.Select(e => e.barcode).ToList<string>();

                    var usedBar = dataB.product_transactions.Select(c => new { c.barcode, c.bill_type }).Where(x => x.bill_type != mBillType && barcodes.Contains<string>(x.barcode));
                    usedBarcodes = usedBar.Select(e => e.barcode).ToList<string>();
                }
                catch
                {

                }
            }
            return usedBarcodes;
        }
    }
}
