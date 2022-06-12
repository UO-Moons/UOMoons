

class entry:

    def __init__ ( self, x, y, z ):

       self.x = x 
       self.y = y 
       self.z = z 
       self.key = x ^ y ^ z

    def __repr__ ( self ):

        return "(%d,%d,%d) = %d" % ( self.x, self.y, self.z, self.key )

dict = {}

def f():

    dupl = 0

    for x in xrange(0,100):

        for y in xrange(0,100):

            for z in xrange(0,100):

                instance = entry( x, y, z )

                if( dict.has_key( instance.key ) ):

                    dupl += 1
                    
                    #previous = dict[instance.key]
                    #print "previous: " + `previous`
                    #print "instance: " + `instance`

                else: 
                    
                    dict[instance.key]=instance

    print dupl

f()
