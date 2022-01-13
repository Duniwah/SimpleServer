using System;
public class ByteArray
{
    //默认大小
    public const int DefaultSize = 1024;
    //初始大小
    private int mInitSize = 0;
    //容量
    private int mCapacity = 0;
    //缓冲区
    public byte[] Bytes;
    //读取位置
    public int ReadIdx = 0;
    //写入位置
    public int WriteIdx = 0;
    //剩余空间
    public int Remain { get { return mCapacity - WriteIdx; } }
    //数据长度
    public int Length { get { return WriteIdx - ReadIdx; } }

    public ByteArray()
    {
        Bytes = new byte[DefaultSize];
        mCapacity = DefaultSize;
        mInitSize = DefaultSize;
        ReadIdx = 0;
        WriteIdx = 0;
    }

    /// <summary>
    /// 检测并移动数据
    /// </summary>
    public void CheckAndMoveBytes()
    {
        if (Length < 8)
        {
            MoveBytes();
        }
    }

    /// <summary>
    /// 移动（拷贝）数据
    /// </summary>
    public void MoveBytes()
    {
        if (ReadIdx < 0)
        {
            return;
        }
        Array.Copy(Bytes, ReadIdx, Bytes, 0, Length);
        WriteIdx = Length;
        ReadIdx = 0;

    }

    /// <summary>
    /// 重设尺寸
    /// </summary>
    /// <param name="size"></param>
    public void ReSize(int size)
    {
        if (ReadIdx < 0) return;
        if (size < Length) return;
        if (size < mInitSize) return;
        int n = 1024;
        while (n < size)
        {
            n *= 2;
        }
        mCapacity = n;
        byte[] newBytes = new byte[mCapacity];
        Array.Copy(Bytes, ReadIdx, newBytes, 0, Length);
        Bytes = newBytes;
        WriteIdx = Length;
        ReadIdx = 0;
    }
}
