using MegaCrit.Sts2.Core.Map;

namespace Ruina2.Ruina2Code.Acts;

public sealed class BlackSilenceActMap : ActMap
{
	private readonly MapPoint?[,] _grid = new MapPoint[7, 17];

	public override MapPoint BossMapPoint { get; }
	public override MapPoint? SecondBossMapPoint { get; }
	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid => _grid;

	public BlackSilenceActMap()
	{
		StartingMapPoint = new MapPoint(3, 0)
		{
			PointType = MapPointType.RestSite,
			CanBeModified = false
		};
		MapPoint mapPoint = Create(3, 1, MapPointType.Elite);
		MapPoint mapPoint1 = Create(3, 3, MapPointType.RestSite);
		MapPoint mapPoint2 = Create(3, 5, MapPointType.Elite);
		MapPoint mapPoint3 = Create(3, 7, MapPointType.RestSite);
		BossMapPoint = new MapPoint(3, 8)
		{
			PointType = MapPointType.Boss,
			CanBeModified = false
		};
		SecondBossMapPoint = new MapPoint(3, 9)
		{
			PointType = MapPointType.Boss,
			CanBeModified = false
		};
		
		StartingMapPoint.AddChildPoint(mapPoint);
		mapPoint.AddChildPoint(mapPoint1);
		mapPoint1.AddChildPoint(mapPoint2);
		mapPoint2.AddChildPoint(mapPoint3);
		mapPoint3.AddChildPoint(BossMapPoint);
		BossMapPoint.AddChildPoint(SecondBossMapPoint);
		startMapPoints.Add(mapPoint);
	}

	private MapPoint Create(int column, int row, MapPointType type)
	{
		MapPoint mapPoint = new MapPoint(column, row)
		{
			PointType = type,
			CanBeModified = false
		};
		_grid[column, row] = mapPoint;
		return mapPoint;
	}
}