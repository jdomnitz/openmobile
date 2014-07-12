using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMBricks
{
    class BallBrickCollision
    {

        public Bricks Brick;
        public bool CollisionX;
        public double CollisionDistanceX;
        public bool CollisionY;
        public double CollisionDistanceY;
        public Paddle p;
        public BallBrickCollision(Bricks brick, bool collisionX, double collisionDistanceX, bool collisionY, double collisionDistanceY, Paddle P = null)
        {
            Brick = brick;
            CollisionX = collisionX;
            CollisionY = collisionY;
            CollisionDistanceX = collisionDistanceX;
            CollisionDistanceY = collisionDistanceY;
            p = P;
        }

    }
}
