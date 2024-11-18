    using Microsoft.IdentityModel.Tokens;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace TyshykWebApp.Models
    {
        public class Computation
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string UserId { get; set; } 
            public string Status { get; set; } = "Pending";
            public int ProgressPercentage { get; set; } = 0;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Result { get; set; } = string.Empty;
            [NotMapped]
            public bool CanCancel { get; set; } = true;

            private CancellationTokenSource _cancellationTokenSource = new();

            public async Task StartCalculationAsync(int rows, int cols)
            {
                try
                {
                    var matA = await GenerateMatrixAsync(rows, cols);
                    var matB = await GenerateMatrixAsync(cols, rows);

                    StartTime = DateTime.UtcNow;
                    Status = "InProgress";

                    var resultMatrix = await Task.Run(() => MultiplyMatrices(matA, matB, _cancellationTokenSource.Token));
                    Status = "Completed";
                    Result = await Task.Run(() => ComputeResult(resultMatrix));
                }
                catch (Exception ex) when (ex is OperationCanceledException)
                {
                    Status = "Cancelled";
                }
                catch (Exception)
                {
                    Status = "Failed";
                }
                finally
                {
                    EndTime = DateTime.UtcNow;
                }
            }

            public string ComputeResult(int[,] resMat)
            {
                int rows = resMat.GetLength(0);
                int cols = resMat.GetLength(1);

                long res = 0;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        res += resMat[i, j];
                    }
                }

                return res.ToString();
            }

            private async Task<int[,]> GenerateMatrixAsync(int rows, int columns)
            {
                return await Task.Run(() =>
                {
                    var random = new Random();
                    int[,] matrix = new int[rows, columns];

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            matrix[i, j] = random.Next(1, 100 + 1);
                        }
                    }

                    return matrix;
                });
            }

            public void Cancel()
            {
                if (CanCancel)
                {
                    _cancellationTokenSource.Cancel();
                }
            }

            private int[,] MultiplyMatrices(int[,] matrixA, int[,] matrixB, CancellationToken cancellationToken)
            {
                int rows = matrixA.GetLength(0);
                int cols = matrixB.GetLength(1);
                int innerDim = matrixA.GetLength(1);
                int[,] result = new int[rows, cols];

            try
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        ProgressPercentage = (int)((i + 1) / (double)rows * 100);
                        cancellationToken.ThrowIfCancellationRequested();

                        int sum = 0;
                        for (int k = 0; k < innerDim; k++)
                        {
                            sum += matrixA[i, k] * matrixB[k, j];
                        }
                        result[i, j] = sum;

                    }

                }

                return result;
            } catch (Exception ex)
            {
                throw new OperationCanceledException();
            }
                
            }
        }
    }
